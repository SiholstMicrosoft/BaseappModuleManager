import vscode from "vscode";
import workspace = vscode.workspace;
import { Service } from "./interfaces/service.interface";
import { BmmCore, FileEvent } from "./bmm-core";
import { TaskRunner } from "../utils/task-runner";
import { getWorkspaceSetup } from "../utils/workspace";

export class WorkspaceSyncService implements Service {  

  init(context: vscode.ExtensionContext): void {
    context.subscriptions.push(workspace.onDidChangeConfiguration(e => this.onConfigUpdate()));    
    context.subscriptions.push(workspace.onDidCreateFiles(e => this.onCreateFiles(e)));
    context.subscriptions.push(workspace.onDidRenameFiles(e => this.onRenameFiles(e)));
    context.subscriptions.push(workspace.onDidDeleteFiles(e => this.onDeleteFiles(e)));
    this.onConfigUpdate();
  }

  private onConfigUpdate() {
    const setup = getWorkspaceSetup();
    if(setup == null) {
      return;
    }
    TaskRunner.run(
      `Syncing workspace to country code ${setup.countryCode}`,
      () => BmmCore.initWorkspace(setup.directory, setup.countryCode),
      true
    );
  }

  private onCreateFiles(e: vscode.FileCreateEvent) {
    const setup = getWorkspaceSetup();
    if(setup == null) {
      return;
    }
    const files = e.files.filter(x => x.fsPath.endsWith('.al'));
    if(files.length === 0) {
      return;
    }
    TaskRunner.run(
      `Syncing created files`,
      () => BmmCore.syncFiles(setup.directory, setup.countryCode, FileEvent.create, files),
      true
    );
  }

  private onRenameFiles(e: vscode.FileRenameEvent) {
    const setup = getWorkspaceSetup();
    if(setup == null) {
      return;
    }
    const files = e.files
      .filter(x => x.oldUri.fsPath.endsWith('.al'))
      .map(x => [x.oldUri, x.newUri]).flat();

    if(files.length === 0) {
      return;
    }

    TaskRunner.run(
      `Syncing renamed files`,
      () => BmmCore.syncFiles(setup.directory, setup.countryCode, FileEvent.rename, files),
      true
    );
  }

  private onDeleteFiles(e: vscode.FileDeleteEvent) {
    const setup = getWorkspaceSetup();
    if(setup == null) {
      return;
    }
    const files = e.files.filter(x => x.fsPath.endsWith('.al'));
    if(files.length === 0) {
      return;
    }

    TaskRunner.run(
      `Syncing deleted files`,
      () => BmmCore.syncFiles(setup.directory, setup.countryCode, FileEvent.delete, files),
      true
    );
  }
}