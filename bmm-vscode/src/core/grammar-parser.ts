import nodeFs from 'fs';
import { IGrammar, INITIAL, IOnigLib, ITokenizeLineResult, parseRawGrammar, Registry } from 'vscode-textmate';
import * as oniguruma from 'vscode-oniguruma' ;
import path from 'path';
import { TextDocument } from 'vscode';

export type TokenizeResult = {
  lineTokens: ITokenizeLineResult[],
  lines: string[]
};

export class GrammarParser {

  private static _grammar: IGrammar;
  get _grammar(): IGrammar {
      return GrammarParser._grammar;
  }

  async tokenizeDocument(document: TextDocument): Promise<TokenizeResult> {
    await this.initialize();

    const lines: string[] = [];
    for(let i = 0; i < document.lineCount; i++) {
      lines.push(document.lineAt(i).text);
    }
    
    let ruleStack = INITIAL;
    const tokenizedLines: ITokenizeLineResult[] = lines.map(line => {
      const lineTokens = this._grammar.tokenizeLine(line, ruleStack);
      ruleStack = lineTokens.ruleStack;
      return lineTokens;
    });
    
    return {
      lineTokens: tokenizedLines,
      lines: lines
    };
  }

  private async initialize() {    
    if(this._grammar != null) {
        return;
    }
    const wasmBin = nodeFs.readFileSync(path.join(__dirname, '../onig.wasm')).buffer;
    const alSyntax = nodeFs.readFileSync(path.join(__dirname, '../alsyntax.tmlanguage.txt')).toString();
    const vscodeOnigurumaLib = oniguruma.loadWASM(wasmBin).then(() => { return {
        createOnigScanner(patterns) { return new oniguruma.OnigScanner(patterns); },
        createOnigString(s) { return new oniguruma.OnigString(s); }
    } as IOnigLib; });

    const registry = new Registry({
      onigLib: vscodeOnigurumaLib,
      loadGrammar: async (scopeName) => parseRawGrammar(alSyntax)
    });    
    GrammarParser._grammar = (await registry.loadGrammar('source.al'))!;
  }
}