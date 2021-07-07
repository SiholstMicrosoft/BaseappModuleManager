import vscode from 'vscode';
import { CountryCode } from '../definitions/country-code';
import { Config } from './config';
import { getWorkspacePath } from './paths';

export class WorkspaceSetup {
  constructor(readonly directory: vscode.Uri, readonly countryCode: CountryCode) { }
}

export function getWorkspaceSetup(): WorkspaceSetup | undefined {
  const workspaceDir = getWorkspacePath();
  const countryCode = Config.getCountryCode();
  if(workspaceDir == null || countryCode == null) {
    return;
  }
  return new WorkspaceSetup(workspaceDir, countryCode);
}