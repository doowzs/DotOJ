import { Component, Input, OnChanges, OnDestroy, OnInit, SimpleChanges } from '@angular/core';
import { interval, Observable, Subject } from 'rxjs';
import { map, take, takeUntil } from 'rxjs/operators';

import { PaginatedList } from '../../../../interfaces/pagination.interfaces';
import { ProblemViewDto } from '../../../../interfaces/problem.interfaces';
import { SubmissionInfoDto } from '../../../../interfaces/submission.interfaces';
import { VerdictStage } from '../../../../consts/verdicts.consts';
import { SubmissionService } from '../../../services/submission.service';
import { AuthorizeService } from '../../../../api-authorization/authorize.service';
import { SubmissionDetailComponent } from '../detail/detail.component';
import { NzDrawerRef, NzDrawerService } from 'ng-zorro-antd/drawer';

@Component({
  selector: 'app-submission-timeline',
  templateUrl: './timeline.component.html',
  styleUrls: ['./timeline.component.css']
})
export class SubmissionTimelineComponent implements OnInit, OnChanges, OnDestroy {
  @Input() public problem: ProblemViewDto;

  public userId: Observable<string>;
  public list: PaginatedList<SubmissionInfoDto>;
  public submissions: SubmissionInfoDto[];
  public submissionDrawer: NzDrawerRef;
  private destroy$ = new Subject();

  constructor(
    private auth: AuthorizeService,
    private service: SubmissionService,
    private drawer: NzDrawerService
  ) {
    this.userId = this.auth.getUser().pipe(map(u => u && u.sub));
  }

  ngOnInit() {
    interval(2000)
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        if (this.submissions && this.submissions.filter(s => {
          return s.verdictInfo.stage === VerdictStage.RUNNING ||
            (s.verdictInfo.stage === VerdictStage.REJECTED && s.score == null);
        }).length > 0) {
          this.updatePendingSubmissions();
        }
      });
    this.service.newSubmission
      .pipe(takeUntil(this.destroy$))
      .subscribe(submission => this.submissions.unshift(submission));
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes.problem) {
      this.submissions = null;
      this.loadSubmissions(changes.problem.currentValue);
    }
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private loadSubmissions(problem: ProblemViewDto): void {
    this.submissions = [];
    this.userId.pipe(take(1)).subscribe(userId => {
      this.service.getPaginatedList(null, userId, null, problem.id, null, 1)
        .subscribe(list => {
          for (let i = 0; i < list.items.length; ++i) {
            this.submissions.unshift(list.items[i]);
          }
        });
    });
  }

  private updatePendingSubmissions(): void {
    const submissionIds = this.submissions.filter(s => {
      return s.verdictInfo.stage === VerdictStage.RUNNING ||
        (s.verdictInfo.stage === VerdictStage.REJECTED && s.score == null);
    }).map(s => s.id);
    this.service.getBatchInfos(submissionIds)
      .subscribe(updatedSubmissions => {
        for (let i = 0; i < updatedSubmissions.length; ++i) {
          const updated = updatedSubmissions[i];
          const submission = this.submissions.find(s => s.id === updated.id);
          if (submission) {
            submission.verdict = updated.verdict;
            submission.verdictInfo = updated.verdictInfo;
            submission.time = updated.time;
            submission.memory = updated.memory;
            submission.failedOn = updated.failedOn;
            submission.score = updated.score;
            submission.progress = updated.progress;
            submission.judgedAt = updated.judgedAt;
          }
        }
      });
  }

  public viewSubmissionDetail(submission: SubmissionInfoDto) {
    this.submissionDrawer = this.drawer.create<SubmissionDetailComponent>({
      nzWidth: '50vw',
      nzTitle: 'Submission #' + submission.id.toString(),
      nzContent: SubmissionDetailComponent,
      nzContentParams: {
        submissionId: submission.id
      }
    });
  }
}
