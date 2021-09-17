import { Component, Input } from '@angular/core';
import { VerdictStage } from '../../consts/verdicts.consts';
import { SubmissionInfoDto, SubmissionViewDto } from '../../interfaces/submission.interfaces';

@Component({
  selector: 'verdict',
  templateUrl: './verdict.component.html',
  styleUrls: ['./verdict.component.css']
})
export class VerdictComponent {
  @Input() simple: boolean = true;
  @Input() badge: boolean = false;
  @Input() submission: SubmissionInfoDto | SubmissionViewDto;

  constructor() {
  }

  public getSubmissionVerdict(): string {
    return this.submission.verdictInfo.name;
  }

  public getSubmissionPct(): string {
    const verdict = this.submission.verdictInfo;
    if (verdict.stage === VerdictStage.RUNNING && this.submission.progress) {
      return this.submission.progress + '%';
    } else if (verdict.stage === VerdictStage.REJECTED && this.submission.score == null && this.submission.progress) {
      return '(Running ' + this.submission.progress + '%)';
    } else if (this.submission.isValid && this.submission.score >= 0 && verdict.showCase) {
      if (verdict.stage === VerdictStage.REJECTED) {
        return (100 - this.submission.score) + '%';
      } else {
        return this.submission.score + '%';
      }
    }
    return null;
  }
}
