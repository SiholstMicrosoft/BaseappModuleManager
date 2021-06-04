import { IToken, ITokenizeLineResult } from "vscode-textmate";
import { ALDocument } from "../models/al-document";
import { GrammarParser } from "./grammar-parser";

const grammarParser = new GrammarParser();
const regexBmmIgnore = /\s*\/\/\s*!bmm-ignore/;
const scopeAlSource = 'source.al';
const scopeKeywordApplicationObject = 'keyword.other.applicationobject.al';
const scopeConstantNumeric = 'constant.numeric.al';
const scopeKeywordControl = 'keyword.control.al';

export async function processForTransfer(document: ALDocument): Promise<string[]> {
  let result = await grammarParser.tokenizeDocument(await document.getAsTextDocument());
  const includedLines: string[] = [];

  for(let i = 0; i < result.lines.length; i++) {
    let lineTokens = result.lineTokens[i];
    let line = result.lines[i];

    if(!regexBmmIgnore.test(line)) {
      includedLines.push(line);
      continue;
    }
    
    i++;
    lineTokens = result.lineTokens[i];      

    if(isObjectDefinition(lineTokens, result.lines[i])) {
      return [];
    }

    let openBeginStatements = 1;
    
    i++;
    while(!isBeginStatement(result.lineTokens[i], result.lines[i])) {
      i++;
    }
    
    i++;
    while(openBeginStatements != 0) {
      if(isBeginStatement(result.lineTokens[i], result.lines[i])) {
        openBeginStatements++;
      }
      else if(isEndStatement(result.lineTokens[i], result.lines[i])) {
        openBeginStatements--;
      }
      i++;
    }
    i--;
  }
  return includedLines;
}

function isObjectDefinition(lineTokens: ITokenizeLineResult, line: string): boolean {
  const tokens = cleanLineTokens(lineTokens);
  return tokens.length >= 2 &&
    tokens[0].scopes[0]  === scopeKeywordApplicationObject &&
    tokens[1].scopes[0]  === scopeConstantNumeric;
}

function isBeginStatement(lineTokens: ITokenizeLineResult, line: string): boolean {
  const tokens = cleanLineTokens(lineTokens);
  return tokens.length >= 1 &&
    tokens[0].scopes[0] === scopeKeywordControl &&
    line.substring(tokens[0].startIndex, tokens[0].endIndex).toLowerCase() === 'begin';
}

function isEndStatement(lineTokens: ITokenizeLineResult, line: string): boolean {
  const tokens = cleanLineTokens(lineTokens);
  return tokens.length >= 1 &&
    tokens[0].scopes[0] === scopeKeywordControl &&
    line.substring(tokens[0].startIndex, tokens[0].endIndex).toLowerCase() === 'end';
}

function cleanLineTokens(lineTokens: ITokenizeLineResult): IToken[] {
  return lineTokens.tokens
    .filter(token => token.scopes.length > 1)
    .map(token => { return {
        startIndex: token.startIndex,
        endIndex: token.endIndex,
        scopes: token.scopes.filter(x => x !== scopeAlSource)
      } as IToken;
    });
}