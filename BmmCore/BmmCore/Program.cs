using BmmCore.Models.Options;
using BmmCore.Processing;
using BmmCore.Workspace;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Synchronizer
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            var host = CreateHostBuilder(args);
            await host.RunConsoleAsync();
            return Environment.ExitCode;


            //            var parser = new BmmParser();
            //            var extProcessor = new BmmExtProcessor();

            //            var parseRequest = new StringParseRequest(
            //@"tableextension 50100 ""Item Ext Test"" extends Item
            //{
            //    fields
            //    {
            //        field(1337; ""Hello World""; Text[10])
            //        {

            //        }
            //    }

            //    var
            //        globalVariable: Integer;

            //    procedure Foo()
            //    var
            //        localVariable: Integer;
            //    begin
            //        globalVariable := 1;
            //        localVariable := globalVariable;
            //        ""Hello World"" := 'Hi!';
            //        // !bmm-ignore
            //        Test(localVariable);

            //        // This is a variable referenced in the original table.
            //        ""No."" := '';
            //    end;

            //    // Only prefix local procedures.
            //    local procedure Bar()
            //    var
            //        localVariable: Boolean;
            //        globalVariable: Integer; // We should identify local variables overriding global variables.
            //    begin
            //        globalVariable := 1;
            //        Foo(); // Comment about Foo, however, this Foo should not change.
            //        localVariable := IsInventoriableType();
            //    end;

            //    // !bmm-ignore
            //    local procedure Test(localVariable: Integer)
            //    begin
            //        Message(""Hello World"" + Format(localVariable));
            //    end;
            //}",
            //                true
            //            );

            //            var parseResult = parser.Parse(parseRequest);
            //            var processRequest = new StringExtProcessingRequest(
            //                (parseResult as StringParseResult).Content,
            //                (SyntaxKind)parseResult.RootKind,
            //                "Test"
            //            );

            //            var isExtensionObject = extProcessor.IsExtensionObject(processRequest);
            //            var isExtensionObjectSupported = extProcessor.IsExtensionObjectSupported(processRequest);

            //            var processResult = extProcessor.Process(processRequest);

            //            Console.WriteLine("Fields:");
            //            foreach (var field in processResult.ExtensionFields)
            //            {
            //                Console.WriteLine(field.ToFullString());
            //            }

            //            Console.WriteLine("Procedures:");
            //            foreach (var procedure in processResult.Procedures)
            //            {
            //                Console.WriteLine(procedure.ToFullString());
            //            }

            //            Console.WriteLine("Global variables:");
            //            foreach (var variable in processResult.GlobalVariables)
            //            {
            //                Console.WriteLine(variable.ToFullString());
            //            }
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureServices(service =>
                    service
                        .AddHostedService<Worker>()
                        .AddSingleton(args)
                        .AddSingleton<IWorkspace, Workspace>()
                        .AddSingleton<IBmmTransferer, BmmTransferer>()
                );
        }

        private class Worker : IHostedService
        {
            private readonly string[] _args;
            private readonly IHostApplicationLifetime _hostApplicationLifetime;
            private readonly IWorkspace _workspace;
            private readonly IBmmTransferer _transferer;

            public Worker(
                string[] args,
                IHostApplicationLifetime hostApplicationLifetime,
                IWorkspace workspace,
                IBmmTransferer transferer
            )
            {
                _args = args;
                _hostApplicationLifetime = hostApplicationLifetime;
                _workspace = workspace;
                _transferer = transferer;
            }

            public Task StartAsync(CancellationToken cancellationToken)
            {
                var task = Parser.Default
                    .ParseArguments<
                        InitializeOptions, 
                        SyncFilesOptions, 
                        CreateLocalizedVersionOptions,
                        TransferOptions
                     >(_args)
                    .WithParsed((IOptions option) => option.Validate())
                    .MapResult(
                        (InitializeOptions opts) => _workspace.Initialize(opts),
                        (SyncFilesOptions opts) => _workspace.SyncFiles(opts),
                        (CreateLocalizedVersionOptions opts) => _workspace.CreateLocalizedVersion(opts),
                        (TransferOptions opts) => _transferer.Transfer(opts),
                        errors => Task.FromResult(1)
                    );

                task.Wait(cancellationToken);
                _hostApplicationLifetime.StopApplication();
                return Task.CompletedTask;
            }

            public Task StopAsync(CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }
        }
    }
}
