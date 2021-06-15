using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Synchronizer.Models;
using Synchronizer.Parsers;
using System.IO;

namespace SynchronizerTests
{
    [TestClass]
    public class BmmParserTests
    {
        [TestMethod]
        [DeploymentItem(@"TestFiles/NoALCodeLeft.al", "TestFiles")]
        public void NoALCodeTestNoComments()
        {
            // Arrange.
            var content = File.ReadAllText(@"TestFiles/NoALCodeLeft.al");
            var request = new StringParseRequest(content, false);
            var parser = new BmmParser();

            // Act.
            var result = parser.Parse(request);

            // Assert.
            Assert.IsInstanceOfType(
                result, 
                typeof(StringParseResult), 
                $"Expected parse result to be of type {nameof(StringParseResult)}"
            );
            Assert.IsNull(result.RootKind, "Expected no root kind as there is no code.");
            Assert.IsFalse(result.HasALCode, "Expected no AL code to be present after parsing.");

            var stringParseResult = result as StringParseResult;
            Assert.AreEqual("", stringParseResult.Content, "Expected no content left after parsing.");
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles/NoALCodeLeft.al", "TestFiles")]
        public void NoALCodeTestWithComments()
        {
            // Arrange.
            var content = File.ReadAllText(@"TestFiles/NoALCodeLeft.al");
            var request = new StringParseRequest(content, true);
            var parser = new BmmParser();

            // Act.
            var result = parser.Parse(request);

            // Assert.
            Assert.IsInstanceOfType(
                result,
                typeof(StringParseResult),
                $"Expected parse result to be of type {nameof(StringParseResult)}"
            );
            Assert.IsFalse(result.HasALCode, "Expected no AL code to be present after parsing.");

            var stringParseResult = result as StringParseResult;
            Assert.IsNull(result.RootKind, "Expected no root kind as there is no code.");
            Assert.AreEqual(
                "// Comment End.", 
                stringParseResult.Content, 
                "Expected only leftover comments to be present after parsing."
            );
        }
       
        [TestMethod]
        [DeploymentItem(@"TestFiles/RemoveProcedure.al", "TestFiles")]
        public void RemoveProcedureWithoutComments()
        {
            // Arrange.
            var content = File.ReadAllText(@"TestFiles/RemoveProcedure.al");
            var request = new StringParseRequest(content, false);
            var parser = new BmmParser();

            // Act.
            var result = parser.Parse(request);

            // Assert.
            Assert.IsInstanceOfType(
                result,
                typeof(StringParseResult),
                $"Expected parse result to be of type {nameof(StringParseResult)}"
            );
            Assert.IsTrue(result.HasALCode, "Expected AL code to be present after parsing.");

            var stringParseResult = result as StringParseResult;

            Assert.AreEqual(
                result.RootKind, 
                SyntaxKind.CodeunitObject, 
                $"Expected root to be ${nameof(SyntaxKind.CodeunitObject)}"
            );

            var expectedContent =
@"codeunit 50100 MyCodeunit
{

  var
    myInt: Integer;
}";

            Assert.AreEqual(
               expectedContent,
               stringParseResult.Content,
               "Expected procedure to be removed after parsing and comments to be removed."
           );
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles/RemoveProcedure.al", "TestFiles")]
        public void RemoveProcedureWithComments()
        {
            // Arrange.
            var content = File.ReadAllText(@"TestFiles/RemoveProcedure.al");
            var request = new StringParseRequest(content, true);
            var parser = new BmmParser();

            // Act.
            var result = parser.Parse(request);

            // Assert.
            Assert.IsInstanceOfType(
                result,
                typeof(StringParseResult),
                $"Expected parse result to be of type {nameof(StringParseResult)}"
            );
            Assert.IsTrue(result.HasALCode, "Expected AL code to be present after parsing.");

            var stringParseResult = result as StringParseResult;

            Assert.AreEqual(
                result.RootKind,
                SyntaxKind.CodeunitObject,
                $"Expected root to be ${nameof(SyntaxKind.CodeunitObject)}"
            );

            var expectedContent =
@"codeunit 50100 MyCodeunit
{

  // Some comment.
  var
    myInt: Integer;
}";

            Assert.AreEqual(
               expectedContent,
               stringParseResult.Content,
               "Expected procedure to be removed after parsing and other comments to be kept."
           );
        }

    }
}
