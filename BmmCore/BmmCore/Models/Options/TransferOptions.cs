using CommandLine;

namespace BmmCore.Models.Options
{
    [Verb("transfer", HelpText = "Transfer current workspace to Baseapp.")]
    public class TransferOptions : IOptions
    {
        [Value(0, Required = true, HelpText = "Directory path of workspace.")]
        public string WorkspaceDirectory { get; set; }

        [Option('n', "NAV root",
            Required = true,
            HelpText = "The root directory path of the NAV project (e.g. C:\\NAV)"
        )]
        public string NAVRoot { get; set; }

        public void Validate()
        {
            throw new System.NotImplementedException();
        }
    }
}
