import * as vscode from 'vscode';
import path from 'path';
import child_process from 'child_process';
import { CountryCode } from '../definitions/country-code';
import { Observable } from 'rxjs';
import { Constants } from '../definitions/constants';
import { TaskResponse } from '../utils/task-runner';

export enum FileEvent {
  create = 'Create',
  delete = 'Delete',
  rename = 'Rename'
}

export abstract class BmmCore {
  private static dllPath: string;

  static init(context: vscode.ExtensionContext) {
    const servicePath = vscode.Uri.file(path.join(context.extensionPath, Constants.bmmCoreDllLocation));
    this.dllPath = servicePath.fsPath;
  }

  static initWorkspace(workspaceDir: vscode.Uri, countryCode: CountryCode): Observable<TaskResponse<number>> {
    return this.run(['init', workspaceDir.fsPath, '-c', countryCode]);
  }

  static syncFiles(
    workspaceDir: vscode.Uri, 
    countryCode: CountryCode, 
    fileEvent: FileEvent, 
    files: vscode.Uri[]
  ): Observable<TaskResponse<number>> {
    return this.run([
      'file-sync', workspaceDir.fsPath, 
      '-c', countryCode, 
      '-e', fileEvent, 
      '-f', files.map(x => x.fsPath).join(' ')
    ]);
  }

  static createLocalizedVersion(
    workspaceDir: vscode.Uri, 
    countryCode: CountryCode, 
    target: CountryCode,
    file: vscode.Uri
  ): Observable<TaskResponse<number>> {
    return this.run([
      'new-localized', workspaceDir.fsPath, 
      '-c', countryCode, 
      '-t', target, 
      '-f', file.fsPath
    ]);
  }

  private static run(args: string | string[]) : Observable<TaskResponse<number>> {
    return new Observable<TaskResponse<number>>(subscriber => {
      let execCommand = `dotnet ${this.dllPath} ${typeof args === 'string' ? args : args.join(' ')}`;
      const process = child_process.exec(execCommand);
      const errors: string[] = [];

      process.stderr?.addListener('data', data => {
        errors.push(data.toString());
      });

      process.on('exit', exitCode => {
        subscriber.next(new TaskResponse(exitCode ?? 0, errors.join('\n')));
        subscriber.complete();
      });
    });
  }
}