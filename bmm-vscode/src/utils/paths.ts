import * as path from 'path';
import * as vscode from 'vscode';
import { Constants } from '../definitions/constants';
import workspace = vscode.workspace;

export function getRelativePath(base: string, filePath: string): string {
  const workspacePath = workspace.workspaceFolders![0].uri.path;
  filePath = filePath.replace(workspacePath, '');
  filePath = filePath.replace(base, '');
  return path.normalize(filePath);
}

export function getFullPath(...relativePath: string[]): string {
  const workspacePath = workspace.workspaceFolders![0].uri.path;
  return path.join(workspacePath, ...relativePath);
}

export function removeFileExtension(filePath: string): string {
  return filePath.replace(Constants.ignoreFileExtension, '').replace('.al', '');
}