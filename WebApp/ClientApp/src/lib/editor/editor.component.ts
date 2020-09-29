import {
  AfterViewChecked,
  Component,
  EventEmitter,
  Input,
  OnChanges,
  OnDestroy,
  OnInit,
  Output,
  SimpleChanges
} from '@angular/core';

import { Program } from '../../interfaces/submission.interfaces';
import { LanguageInfo, Languages } from '../../consts/languages.consts';

import * as ace from 'ace-builds';
import 'ace-builds/src-noconflict/mode-c_cpp';
import 'ace-builds/src-noconflict/mode-csharp';
import 'ace-builds/src-noconflict/mode-golang';
import 'ace-builds/src-noconflict/mode-haskell';
import 'ace-builds/src-noconflict/mode-java';
import 'ace-builds/src-noconflict/mode-python';
import 'ace-builds/src-noconflict/mode-rust';
import { faFolderOpen, faUpload } from '@fortawesome/free-solid-svg-icons';

const EditorLanguageKey: string = 'editor-language';
const EditorCodeKey = (problemId: number): string => 'editor-code-' + problemId.toString();

@Component({
  selector: 'editor',
  templateUrl: './editor.component.html'
})
export class EditorComponent implements OnInit, AfterViewChecked, OnChanges, OnDestroy {
  faFolderOpen = faFolderOpen;
  faUpload = faUpload;
  Languages = Languages;

  @Input() problemId: number;
  @Input() program: Program;
  @Input() readonly: boolean;
  @Input() disabled: boolean;

  @Output() submit = new EventEmitter<Program>();

  private editor: ace.Ace.Editor;
  private language: LanguageInfo;
  private code: string;

  constructor() {
  }

  ngOnInit() {
    this.editor = ace.edit('editor', { useWorker: false, wrap: true });
    if (!!localStorage.getItem(EditorLanguageKey)) {
      this.language = JSON.parse(localStorage.getItem(EditorLanguageKey));
    }
    if (this.language) {
      this.editor.getSession().setMode('ace/mode/' + this.language.mode);
    }
    if (this.problemId) {
      this.loadCode(this.problemId);
    }
  }

  ngAfterViewChecked() {
    // Container of the editor will not be ready in ngOnInit.
    // We need a force resize to avoid layout issues of the editor.
    this.editor.resize(true);
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes.problemId) {
      if (!changes.problemId.isFirstChange()) {
        this.saveCode(changes.problemId.previousValue);
      }
      if (this.editor) {
        this.loadCode(changes.problemId.currentValue);
      }
    }
  }

  ngOnDestroy() {
    this.saveCode(this.problemId);
    this.editor.destroy();
    this.editor.container.remove();
  }

  public changeLanguage(code: string | number) {
    code = Number(code);
    if (!this.language || code !== this.language.code) {
      this.language = Languages.find(l => l.code === code);
      this.editor.getSession().setMode('ace/mode/' + this.language.mode);
      localStorage.setItem(EditorLanguageKey, JSON.stringify(this.language));
    }
  }

  public readFile(event: any) {
    const reader = new FileReader();
    const file = event.target.files[0] as File;
    reader.onload = () => {
      this.editor.setValue(reader.result.toString());
    }
    // Base64 encoding 30720 bytes -> 40960 bytes (4/3)
    if (file.type !== '') {

    } else if (file.size > 30720) {

    } else {
      reader.readAsText(file);
    }
  }

  public loadCode(problemId: number) {
    if (!this.program) {
      this.editor.setValue(localStorage.getItem(EditorCodeKey(problemId)) ?? '');
    }
  }

  public saveCode(problemId: number) {
    if (!this.program) {
      localStorage.setItem(EditorCodeKey(problemId), this.editor.getValue());
    }
  }

  public submitCode() {
    if (this.disabled || !this.language || !this.code) return;
    this.submit.next({
      language: this.language.code,
      code: this.code
    })
  }
}
