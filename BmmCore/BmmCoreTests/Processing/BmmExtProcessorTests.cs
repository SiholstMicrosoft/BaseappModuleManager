using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Synchronizer.Models.Processing;
using Synchronizer.Processing;
using System.IO;

namespace SynchronizerTests
{
    [TestClass]
    public class BmmExtProcessorTests
    {
        [TestMethod]
        [DeploymentItem(@"TestFiles/ExtractTableExtension.al", "TestFiles")]
        public void ExtractTableExtension()
        {
            // Arrange.
            var content = File.ReadAllText(@"TestFiles/ExtractTableExtension.al");
            var request = new StringExtProcessingRequest(content, SyntaxKind.TableExtensionObject, "Prefix");
            var processor = new BmmExtProcessor();

            // Act.
            var result = processor.Process(request);

            // Assert.
            Assert.IsInstanceOfType(
                result,
                typeof(StringExtProcessingResponse),
                $"Expected parse result to be of type {nameof(StringExtProcessingResponse)}"
            );
            Assert.AreEqual(result.ExtensionFields.Count, 2, "Expected 2 extension fields.");
            Assert.AreEqual(result.GlobalVariables.Count, 2, "Expected 2 global variables.");
            Assert.AreEqual(result.Procedures.Count, 2, "Expected 2 procedures.");

            var expectedResult =
@"        field(1337; ""Test Field 1""; Text[10])
        {

        }
";
            Assert.AreEqual(
                expectedResult,
                result.ExtensionFields[0].ToFullString(),
                "Expected field 1 to be extracted with no prefix."
            );

            expectedResult =
@"        field(13337; ""Test Field 2""; Text[10])
        {

        }
";
            Assert.AreEqual(
                expectedResult,
                result.ExtensionFields[1].ToFullString(),
                "Expected field 2 to be extracted with no prefix."
            );

            expectedResult = 
@"        PrefixglobalVariable1: Integer;
";
            Assert.AreEqual(
                expectedResult,
                result.GlobalVariables[0].ToFullString(),
                "Expected global variable 1 to be extracted with prefix."
            );

            expectedResult =
@"        PrefixglobalVariable2: Boolean;
";
            Assert.AreEqual(
                expectedResult,
                result.GlobalVariables[1].ToFullString(),
                "Expected global variable 2 to be extracted with prefix."
            );

            expectedResult =
@"
    procedure Test1()
    begin
    end;
";
            Assert.AreEqual(
                expectedResult,
                result.Procedures[0].ToFullString(),
                "Expected public procedure 1 to be extracted with prefix."
            );

            expectedResult =
@"
    local procedure PrefixTest2()
    begin
    end;
";
            Assert.AreEqual(
                expectedResult,
                result.Procedures[1].ToFullString(),
                "Expected local procedure 2 to be extracted with prefix."
            );
        }
    }
}
