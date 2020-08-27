import { Component, Input, OnDestroy, OnInit } from '@angular/core';
import { forkJoin, interval, Observable, Subject } from 'rxjs';
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
    interval(2000)
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        if (this.pendingSubmissions.length) {
          this.updatePendingSubmissions();
        }
      });
    this.service.newSubmission
      .pipe(takeUntil(this.destroy$))
      .subscribe(submission => this.addNewSubmission(submission));
    this.userId.pipe(take(1)).subscribe(userId => {
      this.service.getPaginatedList(null, this.problemId, userId, null, 1)
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

  private updatePendingSubmissions(): void {
    const observables: Observable<SubmissionInfoDto>[] = [];
    for (let i = 0; i < this.pendingSubmissions.length; ++i) {
      const submission = this.pendingSubmissions[i];
      observables.push(this.service.getSingleAsInfo(submission.id));
    }
    forkJoin(observables).subscribe(updatedSubmissions => {
      for (let i = 0; i < updatedSubmissions.length; ++i) {
        const submission = updatedSubmissions[i];
        if ((submission.verdict as VerdictInfo).stage !== VerdictStage.RUNNING) {
          this.addNewSubmission(submission);
        }
      }
      this.pendingSubmissions = updatedSubmissions.filter(s => (s.verdict as VerdictInfo).stage === VerdictStage.RUNNING);
    });
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
}
