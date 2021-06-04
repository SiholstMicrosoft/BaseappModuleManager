'use strict';

const fs = require('fs');
const path = require('path');

const userPath = process.env.INIT_CWD;
const filesToCopy = [
  'src/definitions/alsyntax.tmlanguage.txt',
  'node_modules/vscode-oniguruma/release/onig.wasm'
];

if (!fs.existsSync('./out')) {
  fs.mkdirSync('./out');
}

filesToCopy.forEach(file => {
  const src = path.join(userPath, file);
  const dest = path.join(userPath, 'out', path.basename(file));
  fs.copyFileSync(src, dest);
});