import {Component, Input, OnInit} from '@angular/core';

import {
  VerdictInfo,
  SubmissionInfoDto,
  SubmissionViewDto
} from 'src/interfaces';
import {Verdicts} from 'src/consts';

@Component({
  selector: 'app-submission-verdict',
  templateUrl: './verdict.component.html'
})
export class SubmissionVerdictComponent implements OnInit {
  @Input() public submission: SubmissionInfoDto | SubmissionViewDto;
  public verdict: VerdictInfo;

  constructor() {
  }

  ngOnInit() {
    this.verdict = Verdicts.find(v => v.code === this.submission.verdict);
  }
}
