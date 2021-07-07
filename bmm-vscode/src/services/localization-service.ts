import vscode from 'vscode';
import { ExtensionContext } from "vscode";
import { Constants } from '../definitions/constants';
import { CountryCode } from '../definitions/country-code';
import { TaskRunner } from '../utils/task-runner';
import { getWorkspaceSetup } from '../utils/workspace';
import { BmmCore } from './bmm-core';
import { Service } from "./interfaces/service.interface";

export class LocalizationService implements Service {
  init(context: ExtensionContext): void {
      context.subscriptions.push(
        vscode.commands.registerCommand(Constants.cmdCreateLocalizedVersion, async () => this.onCreateLocalizedVersion())
      );
  }

  async onCreateLocalizedVersion() {
    const setup = getWorkspaceSetup();
    if(setup == null) {
      return;
    }
    
    const editor = vscode.window.visibleTextEditors.find(x => x.document.fileName.endsWith('.al'));
    if(editor == null) {
      vscode.window.showErrorMessage('No active AL file selected. Please open the AL-file you want to localize.');
      return;
    }
    const options = Object.keys(CountryCode).map(x => CountryCode[x]);
    const countryCode = await vscode.window.showQuickPick(options, { placeHolder: 'Select country code.' });
    if(countryCode == null) {
      return;
    }
    if(editor.document.isDirty || editor.document.isUntitled) {
      vscode.window.showErrorMessage('You must save the document before creating a localized version.');
      return;
    }
    TaskRunner.run(
      `Creating localized version for ${countryCode}`,
      () => BmmCore.createLocalizedVersion(setup.directory, setup.countryCode, countryCode, editor.document.uri),
      true
    );
  }
}