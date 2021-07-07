import * as vscode from 'vscode';
import { BmmCore } from './services/bmm-core';
import { WorkspaceSyncService } from './services/workspace-sync-service';
import { Service } from './services/interfaces/service.interface';
import { CompletionService } from './services/completion-service';
import { LocalizationService } from './services/localization-service';

const services: Service[] = [
  new CompletionService(),
  new LocalizationService(),
  new WorkspaceSyncService()
];

export function activate(context: vscode.ExtensionContext) {
  BmmCore.init(context);
  services.forEach(x => x.init(context));
}

export function deactivate() {
  
}
