import { Component, Input, OnChanges, OnDestroy, OnInit, SimpleChanges } from '@angular/core';
import { ActivatedRoute } from "@angular/router";
import { interval, Observable, Subject } from 'rxjs';
import { map, take, takeUntil } from 'rxjs/operators';

import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { SubmissionDetailComponent } from '../detail/detail.component';

import { PaginatedList } from '../../../../interfaces/pagination.interfaces';
import { SubmissionInfoDto } from '../../../../interfaces/submission.interfaces';
import { VerdictStage } from '../../../../consts/verdicts.consts';
import { ApplicationService } from "../../../services/application.service";
import { SubmissionService } from '../../../services/submission.service';
import { AuthorizeService } from '../../../../api-authorization/authorize.service';
import { ProblemViewDto } from '../../../../interfaces/problem.interfaces';
import { ProblemService } from '../../../services/problem.service';
import { ContestViewDto } from "../../../../interfaces/contest.interfaces";
import { ContestService } from "../../../services/contest.service";
import { faBoxOpen, faClock } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-submission-timeline',
  templateUrl: './timeline.component.html',
  styleUrls: ['./timeline.component.css']
})
export class SubmissionTimelineComponent implements OnInit, OnChanges, OnDestroy {
  faBoxOpen = faBoxOpen;
  faClock = faClock;

  @Input() public problemId: number;
  @Input() public contestantId: string = null;

  public contestId: number;
  public contest: ContestViewDto;
  public problem: ProblemViewDto;

  public userId: Observable<string>;
  public pageIndex: number = 1;
  public totalItems: number = 0;
  public totalPages: number = 1;
  public list: PaginatedList<SubmissionInfoDto>;
  public submissions: SubmissionInfoDto[] = [];
  public averageTime: number = null;
  private destroy$ = new Subject();

  constructor(
    private route: ActivatedRoute,
    private auth: AuthorizeService,
    private applicationService: ApplicationService,
    private submissionService: SubmissionService,
    private problemService: ProblemService,
    private contestService: ContestService,
    private modal: NgbModal
  ) {
    this.contestId = this.route.snapshot.parent.params.contestId;
    this.userId = this.auth.getUser().pipe(take(1), map(u => u && u.sub));
  }

  ngOnInit() {
    this.contestService.getSingle(this.contestId, false)
      .subscribe(contest => {
        this.contest = contest;
      });
    interval(2000)
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        if (this.hasPendingSubmissions()) {
          this.updatePendingSubmissions();
        }
      });
    this.submissionService.newSubmission
      .pipe(takeUntil(this.destroy$))
      .subscribe(submission => {
        this.totalItems++;
        this.submissions.unshift(submission);
        if (this.submissions.length > 15) {
          this.submissions.pop();
          this.totalPages = (this.totalItems + 14) / 15;
        }
        this.loadAverageTime();
      });
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes.problemId) {
      this.pageIndex = 1;
      this.totalItems = 0;
      this.totalPages = 1;
      this.submissions = null;
      this.averageTime = null;
      this.loadProblem(changes.problemId.currentValue);
      this.loadSubmissions(changes.problemId.currentValue);
    }
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  public changePage(pageIndex: number): void {
    this.pageIndex = pageIndex;
    this.loadSubmissions(this.problemId);
  }

  private loadProblem(problemId: number): void {
    this.problemService.getSingle(problemId, false)
      .subscribe(problem => this.problem = problem);
  }

  public loadSubmissions(problemId: number): void {
    if (!!this.contestantId) {
      this.loadSubmissionsInner(problemId, null, this.contestantId);
    } else {
      this.userId.pipe(take(1)).subscribe(userId => {
        this.loadSubmissionsInner(problemId, userId, null);
      });
    }
  }

  private loadSubmissionsInner(problemId: number, userId: string, contestantId: string) {
    this.submissionService.getPaginatedList(null, userId, contestantId, problemId, null, 15, this.pageIndex)
      .subscribe(list => {
        this.totalItems = list.totalItems;
        this.totalPages = list.totalPages;
        this.submissions = list.items;
        if (this.hasPendingSubmissions()) {
          this.loadAverageTime();
        }
      });
  }

  public loadAverageTime(): void {
    this.applicationService.getAverageQueueWaitingTime()
      .subscribe(averageTime => {
        this.averageTime = averageTime;
      });
  }

  private hasPendingSubmissions(): boolean {
    return this.submissions && this.submissions.filter(s => {
      return s.verdictInfo.stage === VerdictStage.RUNNING ||
        (s.verdictInfo.stage === VerdictStage.REJECTED && s.score == null);
    }).length > 0;
  }

  private updatePendingSubmissions(): void {
    const submissionIds = this.submissions.filter(s => {
      return s.verdictInfo.stage === VerdictStage.RUNNING ||
        (s.verdictInfo.stage === VerdictStage.REJECTED && s.score == null);
    }).map(s => s.id);
    this.submissionService.getBatchInfos(submissionIds)
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
            submission.hasMessage = updated.hasMessage;
            submission.viewable = updated.viewable;
          }
        }
        if (!this.hasPendingSubmissions()) {
          this.averageTime = null;
        }
      });
  }

  public viewSubmissionPopup(submission: SubmissionInfoDto): void {
    const modelRef = this.modal.open(SubmissionDetailComponent, {size: 'xl'});
    modelRef.componentInstance.submissionId = submission.id;
    modelRef.componentInstance.standalone = false;
  }
}
