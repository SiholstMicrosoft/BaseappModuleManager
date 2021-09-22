using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BmmCore.Models;
using BmmCore.Parsers;
using System.IO;

namespace SynchronizerTests
{
    [TestClass]
    public class BmmParserTests
    {
        [TestMethod]
        [DeploymentItem(@"TestFiles/NoALCodeTestWithoutComments/Input.al", "TestFiles")]
        [DeploymentItem(@"TestFiles/NoALCodeTestWithoutComments/Expected-Output.al", "TestFiles")]
        public void NoALCodeTestWithoutComments()
        {
            // Arrange.
            var content = File.ReadAllText(@"TestFiles/NoALCodeTestWithoutComments/Input.al");
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
            CheckContent(stringParseResult.Content, @"TestFiles/NoALCodeTestWithoutComments/Expected-Output.al");
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles/NoALCodeTestWithComments/Input.al", "TestFiles")]
        [DeploymentItem(@"TestFiles/NoALCodeTestWithComments/Expected-Output.al", "TestFiles")]
        public void NoALCodeTestWithComments()
        {
            // Arrange.
            var content = File.ReadAllText(@"TestFiles/NoALCodeTestWithComments/Input.al");
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
            Assert.IsNull(result.RootKind, "Expected no root kind as there is no code.");

            var stringParseResult = result as StringParseResult;
            CheckContent(stringParseResult.Content, @"TestFiles/NoALCodeTestWithComments/Expected-Output.al");
        }
       
        [TestMethod]
        [DeploymentItem(@"TestFiles/RemoveProcedureWithoutComments/Input.al", "TestFiles")]
        [DeploymentItem(@"TestFiles/RemoveProcedureWithoutComments/Expected-Output.al", "TestFiles")]
        public void RemoveProcedureWithoutComments()
        {
            // Arrange.
            var content = File.ReadAllText(@"TestFiles/RemoveProcedureWithoutComments/Input.al");
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
            Assert.AreEqual(
                SyntaxKind.CodeunitObject,
                result.RootKind,
                $"Expected root to be ${nameof(SyntaxKind.CodeunitObject)}"
            );

            var stringParseResult = result as StringParseResult;
            CheckContent(stringParseResult.Content, @"TestFiles/RemoveProcedureWithoutComments/Expected-Output.al");
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles/RemoveProcedureWithComments/Input.al", "TestFiles")]
        [DeploymentItem(@"TestFiles/RemoveProcedureWithComments/Expected-Output.al", "TestFiles")]
        public void RemoveProcedureWithComments()
        {
            // Arrange.
            var content = File.ReadAllText(@"TestFiles/RemoveProcedureWithComments/Input.al");
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
            Assert.AreEqual(
                SyntaxKind.CodeunitObject,
                result.RootKind,
                $"Expected root to be ${nameof(SyntaxKind.CodeunitObject)}"
            );

            var stringParseResult = result as StringParseResult;
            CheckContent(stringParseResult.Content, @"TestFiles/RemoveProcedureWithComments/Expected-Output.al");
        }

        private static void CheckContent(string content, string expectedContentFilePath)
        {
            var expectedContent = File.ReadAllText(expectedContentFilePath);
            Assert.AreEqual(expectedContent, content, "Parsed content does not match expected content.");
        }
    }
}
