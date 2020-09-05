import { Component, Input, SimpleChanges } from '@angular/core';
import { SubmissionInfoDto, SubmissionViewDto } from '../../../interfaces/submission.interfaces';
import { VerdictInfo, VerdictStage } from '../../../consts/verdicts.consts';

@Component({
  selector: 'app-submission-verdict',
  templateUrl: './verdict.component.html',
  styleUrls: ['./verdict.component.css']
})
export class SubmissionVerdictComponent {
  @Input() submission: SubmissionInfoDto | SubmissionViewDto;

  constructor() {
  }

  public getSubmissionPct(): string {
    const verdict = this.submission.verdict as VerdictInfo;
    if (verdict.stage === VerdictStage.RUNNING && this.submission.progress) {
      return this.submission.progress + '%';
    } else if (verdict.stage === VerdictStage.REJECTED && this.submission.score == null && this.submission.progress) {
      return '(Running ' + this.submission.progress + '%)';
    } else if (this.submission.failedOn > 0 && this.submission.score >= 0 && verdict.showCase) {
      if (verdict.stage === VerdictStage.REJECTED) {
        return (100 - this.submission.score) + '%';
      } else {
        return this.submission.score + '%';
      }
    }
    return null;
  }

  public notAnValidAttempt = (): boolean => {
    const verdict = this.submission.verdict as VerdictInfo;
    return verdict.stage === VerdictStage.ERROR || (verdict.stage === VerdictStage.REJECTED && this.submission.failedOn === 0);
  };
}
