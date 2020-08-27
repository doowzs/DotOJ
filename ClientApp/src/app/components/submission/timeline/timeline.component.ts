import { Component, Input, OnDestroy, OnInit } from '@angular/core';
import { Observable, Subject } from 'rxjs';
import { map, take, takeUntil } from 'rxjs/operators';
import * as moment from 'moment';

import { VerdictInfo, VerdictStage } from '../../../consts/verdicts.consts';
import { SubmissionService } from '../../../services/submission.service';
import { PaginatedList } from '../../../interfaces/pagination.interfaces';
import { SubmissionInfoDto } from '../../../interfaces/submission.interfaces';
import { AuthorizeService } from '../../../../api-authorization/authorize.service';

@Component({
  selector: 'app-submission-timeline',
  templateUrl: './timeline.component.html',
  styleUrls: ['./timeline.component.css']
})
export class SubmissionTimelineComponent implements OnInit, OnDestroy {
  VerdictStage = VerdictStage;
  @Input() public problemId: number;
  @Input() public contestBeginTime: moment.Moment;
  @Input() public contestEndTime: moment.Moment;

  private destroy$ = new Subject();

  public userId: Observable<string>;
  public list: PaginatedList<SubmissionInfoDto>;

  public pendingSubmissions: SubmissionInfoDto[] = [];
  public practiceSubmissions: SubmissionInfoDto[] = [];
  public contestSubmissions: SubmissionInfoDto[] = [];

  constructor(
    private auth: AuthorizeService,
    private service: SubmissionService
  ) {
    this.userId = this.auth.getUser().pipe(map(u => u && u.sub));
  }

  ngOnInit() {
    this.service.newSubmission
      .pipe(takeUntil(this.destroy$))
      .subscribe(submission => this.addNewSubmission(submission));
    this.userId.pipe(take(1)).subscribe(userId => {
      this.service.getPaginatedList(null, this.problemId, userId, null)
        .subscribe(list => {
          for (let i = 0; i < list.items.length; ++i) {
            this.addNewSubmission(list.items[i]);
          }
        });
    });
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private addNewSubmission(submission: SubmissionInfoDto): void {
    const pending = (submission.verdict as VerdictInfo).stage === VerdictStage.RUNNING;
    if (pending) {
      this.pendingSubmissions.unshift(submission);
    } else {
      if (submission.judgedAt <= this.contestEndTime) {
        this.contestSubmissions.unshift(submission);
      } else {
        this.practiceSubmissions.unshift(submission);
      }
    }
  }

  public getSubmissionTimeString(submission: SubmissionInfoDto): string {
    if (submission.createdAt < this.contestEndTime) {
      return moment.utc((submission.createdAt as moment.Moment).diff(this.contestBeginTime)).format('+HH:mm');
    } else {
      return (submission.createdAt as moment.Moment).format('YYYY-MM-DD HH:mm');
    }
  }
}
