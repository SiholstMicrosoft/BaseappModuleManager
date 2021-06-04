import * as vscode from 'vscode';
import fs = vscode.workspace.fs;
import Uri = vscode.Uri;
import workspace = vscode.workspace;
import * as nodeFs  from 'fs';
import { TextEncoder } from 'util';
import { Constants } from '../definitions/constants';
import { getFullPath, getRelativePath, removeFileExtension } from '../utils/paths';
import { COUNTRY_CODES } from '../definitions/country-codes';

export class ALDocument {
  readonly layerFullPath: vscode.Uri;
  readonly layerPath: vscode.Uri;
  readonly srcFullPath: vscode.Uri;
  readonly srcPath: vscode.Uri;
  readonly relativePath: vscode.Uri;

  constructor(public layer: string, filePath: Uri) {
    const layerPath = vscode.Uri.file(getFullPath(`${Constants.layerFolder}/${this.layer}`));
    const srcPath = vscode.Uri.file(getFullPath(Constants.srcFolder));

    if(filePath.path.startsWith(layerPath.path)) {
      this.layerFullPath = filePath;
      this.layerPath = Uri.file(getRelativePath('', this.layerFullPath.path));
      this.relativePath = Uri.file(
        removeFileExtension(getRelativePath(`${Constants.layerFolder}/${this.layer}`, this.layerFullPath.path))
      );
      this.srcPath = Uri.file(`${Constants.srcFolder}${this.relativePath.path}.al`);
      this.srcFullPath = Uri.file(getFullPath(this.srcPath.path));
    }
    else if(filePath.path.startsWith(srcPath.path)) {
      this.srcFullPath = filePath;
      this.srcPath = Uri.file(getRelativePath('', this.srcFullPath.path));
      this.relativePath = Uri.file(
        removeFileExtension(getRelativePath(`${Constants.srcFolder}/`, this.srcFullPath.path))
        );
      this.layerPath = Uri.file(
        `${Constants.layerFolder}/${this.layer}/${this.relativePath.path}${Constants.ignoreFileExtension}`
      );
      this.layerFullPath = Uri.file(getFullPath(this.layerPath.path));
    }
    else {
      throw new Error('Invalid path. Expected file to be in src- or layer folder.');
    }
  }

  async syncLayerToSrc() {
    const srcFullPath = Uri.file(getFullPath(this.srcPath.path));
    
    if(!nodeFs.existsSync(srcFullPath.fsPath)) {
      await fs.copy(this.layerFullPath, srcFullPath);
    }
    else {
      const textDoc = await workspace.openTextDocument(this.layerFullPath);
      await fs.writeFile(srcFullPath, new TextEncoder().encode(textDoc.getText()));
    }
  }

  async syncSrcToLayer() {
    const srcFullPath = Uri.file(getFullPath(this.srcPath.path));
    
    if(!nodeFs.existsSync(this.layerFullPath.fsPath)) {
      await fs.copy(srcFullPath, this.layerFullPath);
    }
    else {
      const textDoc = await workspace.openTextDocument(srcFullPath);
      await fs.writeFile(this.layerFullPath, new TextEncoder().encode(textDoc.getText()));
    }
  }

  getPathForLayer(layer: string): vscode.Uri {
    const layerFullPath = getFullPath(Constants.layerFolder, layer, this.relativePath.path);
    return vscode.Uri.file(`${layerFullPath}${Constants.ignoreFileExtension}`);
  }

  async rename(newUri: vscode.Uri) {
    const renamedDoc = new ALDocument(this.layer, newUri);
    if(nodeFs.existsSync(this.srcFullPath.fsPath)) {
      await fs.rename(this.srcFullPath, renamedDoc.srcFullPath);
    }

    for(let layer of COUNTRY_CODES) {
      const oldDoc = new ALDocument(layer, this.srcFullPath);
      const newDoc = new ALDocument(layer, renamedDoc.srcFullPath);

      if(nodeFs.existsSync(oldDoc.layerFullPath.fsPath)) {
        await fs.rename(oldDoc.layerFullPath, newDoc.layerFullPath);
      }
    }
  }

  async delete() {
    if(nodeFs.existsSync(this.layerFullPath.fsPath)) {
      await fs.delete(this.layerFullPath);
    }
    if(nodeFs.existsSync(this.srcFullPath.fsPath)) {
      await fs.delete(this.srcFullPath);
    }
  }

  async getAsTextDocument(): Promise<vscode.TextDocument> {
    return await workspace.openTextDocument(this.layerFullPath);
  }
}