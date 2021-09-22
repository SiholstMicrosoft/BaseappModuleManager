using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BmmCore.Models.Processing;
using BmmCore.Processing;
using System.IO;

namespace SynchronizerTests
{
    [TestClass]
    public class BmmExtProcessorTests
    {
        [TestMethod]
        [DeploymentItem(@"TestFiles/ExtractTableExtension/Input.al", "TestFiles")]
        [DeploymentItem(@"TestFiles/ExtractTableExtension/Expected-Field-1.al", "TestFiles")]
        [DeploymentItem(@"TestFiles/ExtractTableExtension/Expected-Field-2.al", "TestFiles")]
        public void ExtractTableExtension()
        {
            // Arrange.
            var content = File.ReadAllText(@"TestFiles/ExtractTableExtension/Input.al");
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
            Assert.AreEqual(2, result.ExtensionFields.Count, "Expected 2 extension fields.");
            Assert.AreEqual(6, result.GlobalVariables.Count, "Expected 6 global variables.");
            Assert.AreEqual(2, result.Procedures.Count, "Expected 2 procedures.");

            CheckContent(
                result.ExtensionFields[0].PrefixedSyntaxNode.ToFullString(), 
                @"TestFiles/ExtractTableExtension/Expected-Field-0.al"
            );
            CheckContent(
                result.ExtensionFields[1].PrefixedSyntaxNode.ToFullString(),
                @"TestFiles/ExtractTableExtension/Expected-Field-1.al"
            );
        }
        private static void CheckContent(string content, string expectedContentFilePath)
        {
            var expectedContent = File.ReadAllText(expectedContentFilePath);
            Assert.AreEqual(expectedContent, content, "Parsed content does not match expected content.");
        }
    }
}
