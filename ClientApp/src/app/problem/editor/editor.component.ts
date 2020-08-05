import {Component, OnInit, OnDestroy, Input} from '@angular/core';
import {ActivatedRoute, Router} from '@angular/router';
import {MatSelectChange} from '@angular/material/select';

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

import {ProblemViewDto} from '../../app.interfaces';
import {ProblemService} from '../problem.service';

const languages: {code, name, mode}[] = [
  {code: 50, name: 'C', mode: 'c_cpp'},
  {code: 150, name: 'C 11', mode: 'c_cpp'},
  {code: 250, name: 'C 14', mode: 'c_cpp'},
  {code: 51, name: 'C#', mode: 'csharp'},
  {code: 54, name: 'C++', mode: 'c_cpp'},
  {code: 154, name: 'C++ 11', mode: 'c_cpp'},
  {code: 254, name: 'C++ 14', mode: 'c_cpp'},
  {code: 354, name: 'C++ 17', mode: 'c_cpp'},
  {code: 60, name: 'Golang', mode: 'golang'},
  {code: 61, name: 'Haskell', mode: 'haskell'},
  {code: 27, name: 'Java 8', mode: 'java'},
  {code: 62, name: 'Java 11', mode: 'java'},
  {code: 63, name: 'JavaScript', mode: 'javascript'},
  {code: 64, name: 'Lua', mode: 'lua'},
  {code: 68, name: 'PHP', mode: 'php'},
  {code: 71, name: 'Python 3', mode: 'python'},
  {code: 72, name: 'Ruby', mode: 'ruby'},
  {code: 73, name: 'Rust', mode: 'rust'},
  {code: 74, name: 'TypeScript', mode: 'typescript'}
];

@Component({
  selector: 'app-problem-code-editor',
  templateUrl: './editor.component.html'
})
export class ProblemCodeEditorComponent implements OnInit, OnDestroy {
  @Input() public problem: ProblemViewDto;

  public languages = languages;
  public currentLanguage: {code, name, mode};
  public code: string;
  private editor: ace.Ace.Editor;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private service: ProblemService
  ) {
  }

  ngOnInit() {
    // TODO: load from data store?
    this.currentLanguage = languages[0];

    this.editor = ace.edit('code-editor', {useWorker: false});
    this.editor.getSession().setMode('ace/mode/' + this.currentLanguage.mode);
  }

  ngOnDestroy() {
    this.editor.destroy();
    this.editor.container.remove();
  }

  public changeLanguage(event: MatSelectChange) {
    if (event.value.code !== this.currentLanguage.code) {
      this.currentLanguage = event.value;
      this.editor.getSession().setMode('ace/mode/' + event.value.mode);
    }
  }
}
