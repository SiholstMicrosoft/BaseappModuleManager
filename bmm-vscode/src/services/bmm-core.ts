import * as vscode from 'vscode';
import path from 'path';
import child_process from 'child_process';

export class BmmCore {
  private dllPath: string;

  constructor(context: vscode.ExtensionContext) {
    const servicePath = vscode.Uri.file(path.join(context.extensionPath, "assets/ALFormatter/ALFormatter.dll"));
    this.dllPath = servicePath.fsPath;
  }

  private run(args: string | string[]) : child_process.ChildProcess  {
    let execCommand = `dotnet ${this.dllPath} ${typeof args === 'string' ? args : args.join(' ')}`;
    return child_process.exec(execCommand);
  }
}