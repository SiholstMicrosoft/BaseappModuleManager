import vscode from 'vscode';

export interface Service {
  init(context: vscode.ExtensionContext): void;
}