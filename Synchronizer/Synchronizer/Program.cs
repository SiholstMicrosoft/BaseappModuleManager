using Microsoft.Dynamics.Nav.CodeAnalysis;
using Synchronizer.Models;
using Synchronizer.Models.Processing;
using Synchronizer.Parsers;
using Synchronizer.Processing;
using System;

namespace Synchronizer
{
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new BmmParserV1();
            var extProcessor = new BmmProcessorV1();

            var parseRequest = new StringParseRequest(
@"tableextension 50100 ""Item Ext Test"" extends Item
{
    fields
    {
        field(1337; ""Hello World""; Text[10])
        {

        }
    }

    var
        globalVariable: Integer;

    procedure Foo()
    var
        localVariable: Integer;
    begin
        globalVariable := 1;
        localVariable := globalVariable;
        ""Hello World"" := 'Hi!';
        // !bmm-ignore
        Test(localVariable);

        // This is a variable referenced in the original table.
        ""No."" := '';
    end;

    procedure Bar()
    var
        localVariable: Boolean;
        globalVariable: Integer; // We should identify local variables overriding global variables.
    begin
        globalVariable := 1;
        Foo(); // Comment about Foo, however, this Foo should not change.
        localVariable := IsInventoriableType();
    end;

    // !bmm-ignore
    local procedure Test(localVariable: Integer)
    begin
        Message(""Hello World"" + Format(localVariable));
    end;
}",
                true
            );

            var parseResult = parser.Parse(parseRequest);
            var processRequest = new StringProcessingRequest(
                (parseResult as StringParseResult).Content,
                (SyntaxKind)parseResult.RootKind,
                "Test"
            );
            var processResult = extProcessor.Process(processRequest);

            Console.WriteLine("Fields:");
            foreach (var field in processResult.ExtensionFields)
            {
                Console.WriteLine(field.ToFullString());
            }

            Console.WriteLine("Procedures:");
            foreach (var procedure in processResult.Procedures)
            {
                Console.WriteLine(procedure.ToFullString());
            }

            Console.WriteLine("Global variables:");
            foreach (var variable in processResult.GlobalVariables)
            {
                Console.WriteLine(variable.ToFullString());
            }
        }
    }
}
