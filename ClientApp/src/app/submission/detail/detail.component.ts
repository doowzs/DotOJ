import {Component, Inject, OnInit} from '@angular/core';
import {MAT_DIALOG_DATA} from '@angular/material/dialog';

import {
  LanguageInfo,
  VerdictInfo,
  SubmissionViewDto
} from 'src/interfaces';
import {Languages, Verdicts} from 'src/consts';
import {SubmissionService} from '../submission.service';
import * as ace from 'ace-builds';

@Component({
  selector: 'app-submission-detail',
  templateUrl: './detail.component.html'
})
export class SubmissionDetailComponent implements OnInit {
  public submission: SubmissionViewDto;
  public language: LanguageInfo;
  public verdict: VerdictInfo;
  private editor: ace.Ace.Editor;

  constructor(
    private service: SubmissionService,
    @Inject(MAT_DIALOG_DATA) private data: { submissionId: number }
  ) {
  }

  ngOnInit() {
    this.service.getSingle(this.data.submissionId)
      .subscribe(submission => {
        console.log(submission);
        this.submission = submission;
        this.verdict = Verdicts.find(v => v.code === submission.verdict);
        this.editor = ace.edit('code-viewer', {useWorker: false, wrap: true, readOnly: true});
        this.language = Languages.find(l => l.code === submission.program.language);
        if (this.language) {
          this.editor.getSession().setMode('ace/mode/' + this.language.mode);
        }
        this.editor.setValue(this.submission.program.code);
      }, error => console.error(error));
  }
}
