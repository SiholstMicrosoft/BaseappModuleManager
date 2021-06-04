import * as vscode from 'vscode';
import { registerBaseAppSync } from './services/base-app-sync';
import { registerCompletionService } from './services/completion';
import { registerFileSyncService, registerFileRenamingService } from './services/file-sync';

export function activate(context: vscode.ExtensionContext) {
  registerCompletionService(context);
  registerFileRenamingService(context);
  registerFileSyncService(context);
  registerBaseAppSync(context);
}

export function deactivate() {}
