import {Component, OnInit, AfterViewChecked, OnChanges, OnDestroy, Input, SimpleChanges} from '@angular/core';
import {ActivatedRoute, Router} from '@angular/router';
import {MatSelectChange} from '@angular/material/select';
import {MatSnackBar} from '@angular/material/snack-bar';

import * as ace from 'ace-builds';
import 'ace-builds/src-noconflict/mode-c_cpp';
import 'ace-builds/src-noconflict/mode-csharp';
import 'ace-builds/src-noconflict/mode-golang';
import 'ace-builds/src-noconflict/mode-haskell';
import 'ace-builds/src-noconflict/mode-java';
import 'ace-builds/src-noconflict/mode-javascript';
import 'ace-builds/src-noconflict/mode-lua';
import 'ace-builds/src-noconflict/mode-php';
import 'ace-builds/src-noconflict/mode-python';
import 'ace-builds/src-noconflict/mode-ruby';
import 'ace-builds/src-noconflict/mode-rust';
import 'ace-builds/src-noconflict/mode-typescript';

import {ProblemViewDto, Languages} from '../../app.interfaces';
import {ProblemService} from '../problem.service';
import {SubmissionService} from '../../submission/submission.service';

@Component({
  selector: 'app-problem-code-editor',
  templateUrl: './editor.component.html'
})
export class ProblemCodeEditorComponent implements OnInit, AfterViewChecked, OnChanges, OnDestroy {
  @Input() public problem: ProblemViewDto;

  public languages = Languages;
  public currentLanguage: { code, name, mode };
  private editor: ace.Ace.Editor;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private service: ProblemService,
    private submitter: SubmissionService,
    private snackBar: MatSnackBar
  ) {
  }

  ngOnInit() {
    this.editor = ace.edit('code-editor', {useWorker: false, wrap: true});
    this.currentLanguage = JSON.parse(localStorage.getItem('editor-language'));
    if (this.currentLanguage) {
      this.editor.getSession().setMode('ace/mode/' + this.currentLanguage.mode);
    }
    this.loadCode(this.problem.id);
  }

  ngAfterViewChecked() {
    // Container of editor will not be ready in ngOnInit.
    // We need a force resize to avoid layout issues of the editor.
    this.editor.resize(true);
  }

  ngOnChanges(changes: SimpleChanges) {
    if (!changes.problem.isFirstChange()) {
      this.saveCode(changes.problem.previousValue.id);
      this.loadCode(changes.problem.currentValue.id);
    }
  }

  ngOnDestroy() {
    this.saveCode(this.problem.id);
    this.editor.destroy();
    this.editor.container.remove();
  }

  public changeLanguage(event: MatSelectChange) {
    if (!this.currentLanguage || event.value !== this.currentLanguage.code) {
      this.currentLanguage = this.languages.find(l => l.code === event.value);
      this.editor.getSession().setMode('ace/mode/' + this.currentLanguage.mode);
      localStorage.setItem('editor-language', JSON.stringify(this.currentLanguage));
    }
  }

  public uploadCode(event: any) {
    const reader = new FileReader();
    reader.onload = () => {
      this.editor.setValue(reader.result.toString());
    };
    reader.readAsText(event.target.files[0]);
  }

  public saveCode(problemId: number, prompt: boolean = false) {
    localStorage.setItem('editor-code-' + problemId, this.editor.getValue());
    if (prompt) {
      this.snackBar.open('Code saved.', 'Done', {
        duration: 2000,
        horizontalPosition: 'left'
      });
    }
  }

  public loadCode(problemId: number) {
    this.editor.setValue(localStorage.getItem('editor-code-' + problemId) ?? '');
  }

  public submitCode(problem: ProblemViewDto) {
    this.submitter.createSingle(problem, this.currentLanguage.code, this.editor.getValue())
      .subscribe(submission => {
        this.snackBar.open('Code submitted as #' + submission.id.toString() + '.', 'Done', {
          duration: 3000,
          horizontalPosition: 'left'
        });
      });
  }
}
