import vscode from 'vscode';
import { ConfigProperty } from '../definitions/config-property';
import workspace = vscode.workspace;
import fs = vscode.workspace.fs;
import { ALDocument } from '../models/al-document';
import { Constants } from '../definitions/constants';
import { COUNTRY_CODES } from '../definitions/country-codes';
import { ALLayer } from '../models/al-layer';
import { getConfig } from '../utils/config';

let alLayer: ALLayer;

export function registerFileRenamingService(context: vscode.ExtensionContext) {
  context.subscriptions.push(
    vscode.commands.registerCommand(Constants.cmdCreateLocalizedVersion, async () => createLocalizedVersion())
  );
  workspace.onDidChangeWorkspaceFolders(() => updateFileNaming());
  updateFileNaming();
}

export function registerFileSyncService(context: vscode.ExtensionContext) {
  context.subscriptions.push(workspace.onDidChangeConfiguration(async e => {
    if (e.affectsConfiguration(ConfigProperty.countryCode)) {
      await syncLayer();
    }
  }));
  context.subscriptions.push(workspace.onDidSaveTextDocument(async e => syncUpdatedFile(e)));
  context.subscriptions.push(workspace.onDidCreateFiles(async e => syncCreatedFiles(e)));
  context.subscriptions.push(workspace.onDidRenameFiles(async e => syncRenamedFiles(e)));
  context.subscriptions.push(workspace.onDidDeleteFiles(async e => syncDeletedFiles(e)));
}

async function createLocalizedVersion() {
  const editor = vscode.window.visibleTextEditors.find(x => x.document.fileName.endsWith('.al'));
  if(editor == null) {
    return;
  }
  const countryCode = await vscode.window.showQuickPick(COUNTRY_CODES, { placeHolder: 'Select country code.' });
  if(countryCode == null) {
    return;
  }
  if(editor.document.isDirty || editor.document.isUntitled) {
    await vscode.window.showErrorMessage('You must save the document before creating a localized version.');
    return;
  }  
  const document = await alLayer.getFile(editor.document.uri);
  if(document == null) {
    return;
  }    
  await fs.copy(document.layerFullPath, document.getPathForLayer(countryCode));
  await syncLayer();
  await vscode.window.showInformationMessage('Localized version created successfully.');
}

async function updateFileNaming() {
  const files = await workspace.findFiles(`${Constants.layerFolder}/**/*.al`);
  files.forEach(file => {
    const newPath = vscode.Uri.file(file.path.replace(/.al$/, Constants.ignoreFileExtension));
    fs.rename(file, newPath);
  });
  await syncLayer();
}

async function syncLayer() {
  if(!hasWorkspaceFolderOpen()) {
    return;
  }
  const layer = getConfig().countryCode;
  alLayer = new ALLayer(layer, true);
  
  const files = await alLayer.getLayerFiles();
  for(let file of files) {
    await file.syncLayerToSrc();
  }
}

async function syncUpdatedFile(textDocument: vscode.TextDocument) {
  if(textDocument.fileName.endsWith(Constants.ignoreFileExtension)) {
    const document = await alLayer.getFile(textDocument.uri);
    await document?.syncLayerToSrc();
  }
  else if(textDocument.fileName.endsWith('.al')) {
    const document = await alLayer.getFile(textDocument.uri);
    await document?.syncSrcToLayer();
  }
}

async function syncCreatedFiles(event: vscode.FileCreateEvent) {
  for(let file of event.files) {
    if(file.path.endsWith('.al')) {
      const document = new ALDocument(Constants.w1Layer, file);
      await document.syncSrcToLayer();
    }
  }
  await updateFileNaming();
}

async function syncRenamedFiles(event: vscode.FileRenameEvent) {
  for(let file of event.files) {
    const document = await alLayer.getFile(file.oldUri);
    await document?.rename(file.newUri);
  }
  await syncLayer();
}

async function syncDeletedFiles(event: vscode.FileDeleteEvent) {
  for(let file of event.files)  {
    const document = await alLayer.getFile(file);
    await document?.delete();
  }
  await syncLayer();  
}

function hasWorkspaceFolderOpen(): boolean {
  return workspace.workspaceFolders != null;
}