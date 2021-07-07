import * as vscode from 'vscode';
import workspace = vscode.workspace;

export function getWorkspacePath(): vscode.Uri | undefined {
  if(!workspace.workspaceFolders?.length) {
    vscode.window.showErrorMessage('No workspace folder available.');
    return;
  }
  if(workspace.workspaceFolders.length > 1) {
    vscode.window.showErrorMessage('Multiple workspaces are not supported.');
    return;
  }
  return workspace.workspaceFolders[0].uri;
}
