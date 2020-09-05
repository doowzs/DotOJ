import { Component, Input } from '@angular/core';
import { SubmissionEditDto, SubmissionInfoDto, SubmissionViewDto } from '../../../interfaces/submission.interfaces';
import { VerdictInfo, VerdictStage } from '../../../consts/verdicts.consts';

@Component({
  selector: 'app-submission-verdict',
  templateUrl: './verdict.component.html',
  styleUrls: ['./verdict.component.css']
})
export class SubmissionVerdictComponent {
  @Input() submission: SubmissionInfoDto | SubmissionViewDto | SubmissionEditDto;

  constructor() {
  }

  public getSubmissionPct(submission: SubmissionInfoDto | SubmissionViewDto): string {
    const verdict = submission.verdict as VerdictInfo;
    if (verdict.stage === VerdictStage.RUNNING && submission.progress) {
      return submission.progress + '%';
    } else if (verdict.stage === VerdictStage.REJECTED && submission.score == null && submission.progress) {
      return '(Running ' + submission.progress + '%)';
    } else if (submission.failedOn > 0 && submission.score >= 0 && verdict.showCase) {
      if (verdict.stage === VerdictStage.REJECTED) {
        return (100 - submission.score) + '%';
      } else {
        return submission.score + '%';
      }
    }
    return null;
  }

  public notAnValidAttempt = (submission: SubmissionInfoDto | SubmissionViewDto): boolean => {
    const verdict = submission.verdict as VerdictInfo;
    return verdict.stage === VerdictStage.ERROR || (verdict.stage === VerdictStage.REJECTED && submission.failedOn === 0);
  };
}
