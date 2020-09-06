import { Component, ElementRef, Input, OnInit, ViewChild } from '@angular/core';

import { LanguageInfo, Languages } from '../../../consts/languages.consts';
import { SubmissionService } from '../../../services/submission.service';
import { NzNotificationService } from 'ng-zorro-antd/notification';
import { ProblemViewDto } from '../../../interfaces/problem.interfaces';

@Component({
  selector: 'app-submission-creator',
  templateUrl: './creator.component.html',
  styleUrls: ['./creator.component.css']
})
export class SubmissionCreatorComponent implements OnInit {
  readonly Languages = Languages;
  readonly languageStorageKey = 'app-submission-creator-language';

  @Input() public problem: ProblemViewDto;
  @ViewChild('sourceFileInput') sourceFileInput: ElementRef;
  @ViewChild('sourceCodeTextArea') sourceCodeTextArea: ElementRef;

  public language: number;
  public factor: number;
  public filename: string;
  public code: string;
  public visible = false;
  public submitting = false;

  constructor(
    private service: SubmissionService,
    private notification: NzNotificationService
  ) {
  }

  ngOnInit() {
    if (localStorage.getItem(this.languageStorageKey)) {
      this.selectLanguage(JSON.parse(localStorage.getItem(this.languageStorageKey)));
    }
  }

  public selectLanguage(value: number) {
    const l = Languages.find(l => l.code === value);
    if (l != null) {
      this.language = value;
      this.factor = l.factor;
      localStorage.setItem(this.languageStorageKey, JSON.stringify(value));
    } else {
      this.language = this.factor = null;
      localStorage.removeItem(this.languageStorageKey);
    }
  }

  public selectSourceFile(event: any) {
    if (!event.target.files.length) {
      this.code = this.filename = null;
    } else {
      const file = event.target.files[0] as File;
      const reader = new FileReader();
      reader.onload = () => {
        this.code = reader.result.toString();
        this.filename = file.name;
      };
      // Base64 encoding 30720 bytes -> 40960 bytes (4/3)
      if (file.type !== '') {
        alert('File ' + file.name + ' has unsupported file type ' + file.type + '.');
      } else if (file.size > 30720) {
        alert('File ' + file.name + 'is too large. Maximum size is 30KiB.');
      } else {
        reader.readAsText(event.target.files[0]);
        console.log(event.target.files[0]);
      }
    }
  }

  public clearSourceFile(event: any) {
    this.code = this.filename = null;
    this.sourceFileInput.nativeElement.value = '';
  }

  public makeSubmission(): void {
    if (this.language && this.code) {
      this.submitting = true;
      this.service.createSingle(this.problem.id, this.language, this.code)
        .subscribe(submission => {
          this.notification.create('success', 'Submitted', 'Code submitted as #' + submission.id.toString() + '.');
          this.clearSourceFile(null);
          this.submitting = this.visible = false;
        }, error => {
          console.log(error);
          this.notification.create('error', 'Error', 'Submitting failed: ' + error.error);
          this.submitting = false;
        });
    }
  }
}
