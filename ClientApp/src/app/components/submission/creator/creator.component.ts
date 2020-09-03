import { Component, ElementRef, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';

import { LanguageInfo, Languages } from '../../../consts/languages.consts';
import { SubmissionService } from '../../../services/submission.service';
import { NzNotificationService } from 'ng-zorro-antd/notification';

@Component({
  selector: 'app-submission-creator',
  templateUrl: './creator.component.html',
  styleUrls: ['./creator.component.css']
})
export class SubmissionCreatorComponent implements OnInit {
  readonly Languages = Languages;
  readonly languageStorageKey = 'app-submission-creator-language';

  @Input() public problemId: number;
  @ViewChild('sourceFileInput') sourceFileInput: ElementRef;
  @Output() public languageChanged = new EventEmitter<LanguageInfo>();

  public language: number;
  public filename: string;
  public code: string;

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
    this.language = value;
    localStorage.setItem(this.languageStorageKey, JSON.stringify(value));
    this.languageChanged.emit(Languages.find(l => l.code === value));
  }

  public selectSourceFile(event: any) {
    if (!event.target.files.length) {
      this.code = this.filename = null;
    } else {
      const reader = new FileReader();
      reader.onload = () => {
        this.code = reader.result.toString();
        this.filename = event.target.files[0].name;
      };
      reader.readAsText(event.target.files[0]);
    }
  }

  public makeSubmission(): void {
    if (this.language && this.code) {
      this.service.createSingle(this.problemId, this.language, this.code)
        .subscribe(submission => {
          this.notification.create('success', 'Submitted', 'Code submitted as #' + submission.id.toString() + '.');
          this.code = this.filename = null;
          this.sourceFileInput.nativeElement.value = '';
        }, error => {
          console.log(error);
          this.notification.create('error', 'Error', 'Submitting failed: ' + error.error);
        });
    }
  }
}
