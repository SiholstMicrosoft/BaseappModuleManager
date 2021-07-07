import * as vscode from 'vscode';
import { ConfigProperty } from '../definitions/config-property';
import { CountryCode } from '../definitions/country-code';
import workspace = vscode.workspace;

export type ConfigOption = { 
  countryCode: string,
  navRoot: string,
  moduleName: string
};

export class Config {
  static getConfig(): ConfigOption {
    const config = workspace.getConfiguration();
    return {
      countryCode: config.get(ConfigProperty.countryCode) ?? '',
      navRoot: config.get(ConfigProperty.navRoot) ?? '',
      moduleName: config.get(ConfigProperty.moduleName) ?? '',
    };
  }

  static getCountryCode(): CountryCode | undefined {
    const countryCode = this.getConfig().countryCode;
    const enumResult = CountryCode[countryCode as keyof typeof CountryCode];
    if(enumResult == null) {
      if(countryCode == null || countryCode.trim() === '') {
        vscode.window.showErrorMessage('Country code is not set for the workspace.');
      }
      else {
        vscode.window.showErrorMessage(`Country code ${countryCode} is not a valid option.`);
      }
    }
    return enumResult;
  }
}