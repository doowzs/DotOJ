import {Component, OnInit, OnDestroy} from '@angular/core';
import {ActivatedRoute, Router} from '@angular/router';
import {ProblemService} from '../problem.service';

import * as ace from 'ace-builds';
import 'ace-builds/src-noconflict/mode-c_cpp';

@Component({
  selector: 'app-problem-code-editor',
  templateUrl: './editor.component.html'
})
export class ProblemCodeEditorComponent implements OnInit, OnDestroy {
  public options = {theme: 'vs-dark'};
  public code: string;
  private editor: ace.Ace.Editor;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private service: ProblemService
  ) {
  }

  ngOnInit() {
    this.editor = ace.edit('code-editor');
    this.editor.getSession().setMode('ace/mode/c_cpp');
  }

  ngOnDestroy() {
    this.editor.destroy();
    this.editor.container.remove();
  }
}
