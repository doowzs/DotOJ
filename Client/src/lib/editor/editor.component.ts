import {
  AfterViewChecked, AfterViewInit, ChangeDetectorRef,
  Component,
  EventEmitter,
  Input,
  OnChanges,
  OnDestroy,
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
import { faCheck, faFolderOpen, faUpload } from '@fortawesome/free-solid-svg-icons';
import { Base64 } from 'js-base64';
import { Title } from '@angular/platform-browser';
import { kMaxLength } from 'buffer';

const EditorLanguageKey: string = 'editor-language';
const EditorCodeKey = (problemId: number): string => 'editor-code-' + problemId.toString();

@Component({
  selector: 'editor',
  templateUrl: './editor.component.html'
})
export class EditorComponent implements AfterViewInit, AfterViewChecked, OnChanges, OnDestroy {
  faCheck = faCheck;
  faFolderOpen = faFolderOpen;
  faUpload = faUpload;
  Languages = Languages.filter(l => !!l.mode);

  static globalId: number = 0;

  public instanceId: string;

  @Input() problemId: number;
  @Input() submissionId: number;
  @Input() program: Program;
  @Input() disabled: boolean;

  @Output() submit = new EventEmitter<Program>();

  public editor: ace.Ace.Editor;
  public language: LanguageInfo;

  constructor(
    private title: Title,
    private cdRef: ChangeDetectorRef
  ) {
    this.instanceId = 'editor-' + (++EditorComponent.globalId).toString();
    if (this.program) {
      this.title.setTitle('Submission #' + this.submissionId);
    }
  }

  ngAfterViewInit() {
    this.editor = ace.edit(this.instanceId, { useWorker: false, wrap: true });
    if (this.program) {
      this.language = Languages.find(l => l.code === this.program.language);
      this.editor.getSession().setMode('ace/mode/' + this.language.mode);
      this.editor.setValue(this.program.code);
      this.editor.setReadOnly(true);
      this.editor.clearSelection();
    } else {
      if (!!localStorage.getItem(EditorLanguageKey)) {
        this.language = JSON.parse(localStorage.getItem(EditorLanguageKey));
        if (this.language) {
          this.editor.getSession().setMode('ace/mode/' + this.language.mode);
        }
      }
      if (this.problemId) {
        this.loadCode(this.problemId);
      }
    }
  }

  ngAfterViewChecked() {
    // Container of the editor will not be ready in ngOnInit.
    // We need a force resize to avoid layout issues of the editor.
    this.editor.resize(false);
    this.cdRef.detectChanges();
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
    if (this.disabled || !this.language || !this.editor.getValue()) return;
    this.saveCode(this.problemId);
    this.submit.next({
      language: this.language.code,
      code: Base64.encode(this.editor.getValue())
    });
  }
}
