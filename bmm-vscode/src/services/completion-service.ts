import vscode from 'vscode';
import { Service } from './interfaces/service.interface';

export class CompletionService implements Service {
  init(context: vscode.ExtensionContext): void {
    context.subscriptions.push(vscode.languages.registerCompletionItemProvider(
      [{  language: 'al' }],
      {
        provideCompletionItems(document: vscode.TextDocument, position: vscode.Position) {
          const linePrefix = document.lineAt(position).text;		
          if (!linePrefix.includes('//') || linePrefix.indexOf('//') > linePrefix.indexOf('!')) {
            return undefined;
          }
          return [
            new vscode.CompletionItem('bmm-ignore', vscode.CompletionItemKind.Text),
          ];
        }
      },
      '!'
    ));
  }
}