import vscode from 'vscode';
import fs = vscode.workspace.fs;
import path from 'path';
import { Constants } from '../definitions/constants';
import { getConfig } from '../utils/config';
import glob from 'glob';
import nReadLines from 'n-readlines';
import { sleep } from '../utils/sleep';
import { COUNTRY_CODES } from '../definitions/country-codes';
import { ALLayer } from '../models/al-layer';
import { TextEncoder } from 'util';
import { processForTransfer } from '../core/transfer-processing';
import { ALDocument } from '../models/al-document';
import child_process from 'child_process';

export function registerBaseAppSync(context: vscode.ExtensionContext) {
  context.subscriptions.push(
    vscode.commands.registerCommand(Constants.cmdTransferToBaseApp, async () => transferToBaseApp(context))
  );
}

const objectDefRegex = /^(\w+)\s+(\d+)\s+[\w\W]+/i;
let idSets: { [objectType: string]: Set<string> };
let assignedIds: { [file: string]: string };

async function transferToBaseApp(context: vscode.ExtensionContext) {
  
  const servicePath = vscode.Uri.file(path.join(context.extensionPath, "assets/ALFormatter/ALFormatter.dll"));
  let execCommand = `dotnet ${servicePath.fsPath}`;

  const formatter = child_process.exec(execCommand);

  let i = 0;
  let str = '';

  formatter.stdout?.on('data', (data) => {
    str += data;
    i++;
    if(i == 5) {
      formatter.kill();
    }
  });

  formatter.on('exit', (code, signal) => {
    vscode.window.showInformationMessage(`Exited with signal ${signal}. Received data: ${str}`);
  });

  return;
  idSets = {};
  assignedIds = {};

  await vscode.window.withProgress({
    location: vscode.ProgressLocation.Notification,
    title: 'Analyzing BaseApp',
    cancellable: false
  }, async (progress) => {

    progress.report({ message: 'Fetching files...' });
    const files = await getBaseAppFiles();

    progress.report({ message: 'Resolving ids...', increment: 0 });
    const incrementProgressAt = files.length / 100;
    let n = 0;
    
    for(let file of files) {
      fetchId(file);
      if(++n >= incrementProgressAt) {
        progress.report({ message: 'Resolving ids...', increment: (n * 100) / files.length });
        await sleep(0);
        n = 0;
      }
    };
  });

  await vscode.window.withProgress({
    location: vscode.ProgressLocation.Notification,
    title: 'Syncing module with BaseApp',
    cancellable: false
  }, async (progress) => {
    for(let layer of COUNTRY_CODES) {
      progress.report({ message: `Syncing layer ${layer}...` });
      
      const targetFolder = `${getConfig().navRoot}/App/Layers/${layer}/BaseApp/Modules/${getConfig().moduleName}`;
      const alLayer = new ALLayer(layer, false);
      const files = await alLayer.getLayerFiles();
      
      for(let file of files) {
        const targetPath = path.join(targetFolder, file.relativePath.fsPath + '.al');
        const lines = await processForTransfer(file);
        if(lines.length === 0) {
          continue;
        }
        assignId(lines, file);
        await fs.writeFile(vscode.Uri.file(targetPath), new TextEncoder().encode(lines.join('\r')));
      }      
      progress.report({ message: `Syncing layer ${layer}...`, increment: 100 / COUNTRY_CODES.length });
      await sleep(0);
    }    
  });

  console.log(assignedIds)
}

function getBaseAppFiles(): Promise<string[]> {
  return new Promise<string[]>(resolve => {
    const navRoot = getConfig().navRoot;
    let files: string[];

    glob(`${navRoot}/App/**/*.al`, (err, result) => {
      files = result;
      resolve(files);
    });
  });
}

function fetchId(file: string) {
  const document = new nReadLines(file);
  let line: Buffer | false;

  while(line = document.next()) {
    const text = line.toString();
    if(objectDefRegex.test(text)) {
      const result = objectDefRegex.exec(text);
      const type = result![1].toLowerCase();
      const id = result![2];
      idSets[type] = idSets[type] || new Set<string>();
      idSets[type].add(id);
    }
  }
}

function assignId(content: string[], file: ALDocument) {
  for(let i = 0; i < content.length; i++) {
    const text = content[i];
    if(objectDefRegex.test(text)) {
      const result = objectDefRegex.exec(text);
      const type = result![1].toLowerCase();
      const id = getNextAvailableId(idSets[type], file);
      idSets[type].add(id);
      assignedIds[file.relativePath.path] = id;
      content[i] = text.replace(/\d+/, id);
    }
  }
}

function getNextAvailableId(set: Set<string>, file: ALDocument): string {
  if(assignedIds[file.relativePath.path] != null) {
    return assignedIds[file.relativePath.path];
  }
  let n = 100;
  while(set.has(n.toString())) {
    n++;
  }
  return n.toString();
}