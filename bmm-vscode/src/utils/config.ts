import * as vscode from 'vscode';
import { ConfigProperty } from '../definitions/config-property';
import workspace = vscode.workspace;

export type Config = { 
  countryCode: string,
  navRoot: string,
  moduleName: string
};

export function getConfig(): Config {
  const config = workspace.getConfiguration();
  return {
    countryCode: config.get(ConfigProperty.countryCode) ?? '',
    navRoot: config.get(ConfigProperty.navRoot) ?? '',
    moduleName: config.get(ConfigProperty.moduleName) ?? '',
  };
}