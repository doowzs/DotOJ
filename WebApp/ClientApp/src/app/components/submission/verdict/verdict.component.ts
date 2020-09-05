import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { SubmissionInfoDto, SubmissionViewDto } from '../../../interfaces/submission.interfaces';
import { VerdictInfo, VerdictStage } from '../../../consts/verdicts.consts';

@Component({
  selector: 'app-submission-verdict',
  templateUrl: './verdict.component.html',
  styleUrls: ['./verdict.component.css']
})
export class SubmissionVerdictComponent implements OnChanges {
  @Input() submission: SubmissionInfoDto | SubmissionViewDto;
  public verdict: VerdictInfo;

  constructor() {
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes.submission) {
      this.verdict = changes.submission.currentValue.verdict as VerdictInfo;
    }
  }

  public getSubmissionPct(): string {
    if (this.verdict.stage === VerdictStage.RUNNING && this.submission.progress) {
      return this.submission.progress + '%';
    } else if (this.verdict.stage === VerdictStage.REJECTED && this.submission.score == null && this.submission.progress) {
      return '(Running ' + this.submission.progress + '%)';
    } else if (this.submission.failedOn > 0 && this.submission.score >= 0 && this.verdict.showCase) {
      if (this.verdict.stage === VerdictStage.REJECTED) {
        return (100 - this.submission.score) + '%';
      } else {
        return this.submission.score + '%';
      }
    }
    return null;
  }

  public notAnValidAttempt = (): boolean => {
    return this.verdict.stage === VerdictStage.ERROR || (this.verdict.stage === VerdictStage.REJECTED && this.submission.failedOn === 0);
  };
}
