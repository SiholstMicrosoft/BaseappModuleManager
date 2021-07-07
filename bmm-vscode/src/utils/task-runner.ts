import vscode from 'vscode';
import { Observable } from "rxjs";

export class TaskResponse<T> {
  constructor(
    public readonly result: T,
    public readonly error: string
  ) { }

  get successful() {
    return !this.error;
  }

  toString(): string {
    if(!this.successful) {
      return `Error: ${this.error}`;
    }
    return '';
  }
}

export abstract class TaskRunner {
  static run<T>(
    title: string,
    task: (progress: vscode.Progress<{ message?: string; increment?: number }>) => Observable<TaskResponse<T>>,
    retry: boolean
  ) {
    vscode.window.withProgress({
      location: vscode.ProgressLocation.Notification,
      title: title,
      cancellable: false
    }, async (progress) => {
      task(progress).subscribe(response => {
        if(response.successful) {
          return;
        }
        if(retry) {
          vscode.window.showErrorMessage(`Task '${title} failed. ${response}`, 'Retry', 'Cancel').then(action => {
            if(action === 'Retry') {
              TaskRunner.run(title, task, retry);
            }
          });
        }
        else if(!response.successful) {
          vscode.window.showErrorMessage(`Task '${title} failed. ${response}`);          
        }
      });
    });
  }
}