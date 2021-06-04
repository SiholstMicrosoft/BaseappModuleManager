import { ALDocument } from './al-document';
import * as vscode from 'vscode';
import workspace = vscode.workspace;
import { Constants } from '../definitions/constants';

export class ALLayer {

  private _files?: ALDocument[];

  constructor(
    public readonly layer: string,
    public readonly includeW1: boolean
  ) { }

  async getLayerFiles(): Promise<ALDocument[]> {
    if(this._files != null) {
      return this._files;
    }
    const files: ALDocument[] = [];

    const w1Path = `${Constants.layerFolder}/W1`;
    const w1Files = this.includeW1 ? await workspace.findFiles(`${w1Path}/**/*${Constants.ignoreFileExtension}`) : [];

    const layerPath =  `${Constants.layerFolder}/${this.layer}`;
    const layerFiles = await workspace.findFiles(`${layerPath}/**/*${Constants.ignoreFileExtension}`);
    
    layerFiles.forEach(file => files.push(new ALDocument(this.layer, file)));
    const addedFiles = new Set(files.map(file => file.relativePath.path));

    w1Files.forEach(file => {
      const document = new ALDocument(Constants.w1Layer, file);
      if(!addedFiles.has(document.relativePath.path)) {
        files.push(document);
      }
    });

    this._files = files;
    return this._files;
  }

  async getFile(path: vscode.Uri): Promise<ALDocument | undefined> {
    const files = await this.getLayerFiles();
    if(path.path.endsWith(Constants.ignoreFileExtension)) {
      return files.find(x => path.path.endsWith(x.layerPath.path));
    }
    else {
      return files.find(x => path.path.endsWith(x.srcPath.path));
    }
  }
}