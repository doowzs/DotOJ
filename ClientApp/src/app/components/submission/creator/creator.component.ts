import { Component, Input, OnInit } from '@angular/core';
import { Languages } from '../../../consts/languages.consts';
import { SubmissionService } from '../../../services/submission.service';

@Component({
  selector: 'app-submission-creator',
  templateUrl: './creator.component.html',
  styleUrls: ['./creator.component.css']
})
export class SubmissionCreatorComponent implements OnInit {
  readonly Languages = Languages;
  readonly languageStorageKey = 'app-submission-creator-language';

  @Input() public problemId: number;

  public language: number;
  public filename: string;
  public code: string;

  constructor(
    private service: SubmissionService
  ) {
  }

  ngOnInit() {
    this.language = JSON.parse(localStorage.getItem(this.languageStorageKey));
  }

  public selectLanguage(value: number) {
    localStorage.setItem(this.languageStorageKey, JSON.stringify(value));
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
      this.service.createSingle(this.problemId, this.language, this.code);
    }
  }
}
