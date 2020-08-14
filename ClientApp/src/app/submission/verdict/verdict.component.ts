import {Component, Input, OnInit} from '@angular/core';

import {
  Verdicts,
  SubmissionInfoDto,
  SubmissionViewDto
} from '../../app.interfaces';

@Component({
  selector: 'app-submission-verdict',
  templateUrl: './verdict.component.html'
})
export class SubmissionVerdictComponent implements OnInit {
  @Input() public submission: SubmissionInfoDto | SubmissionViewDto;
  public verdict: { code: number, name: string, showCase: boolean, stage: number, explain: string };

  constructor() {
  }

  ngOnInit() {
    this.verdict = Verdicts.find(v => v.code === this.submission.verdict) ?? {
      code: 0, name: 'Unknown Verdict', showCase: false, stage: -10000, explain: ''
    };
  }
}
