import { Component, Input, OnChanges, OnDestroy, OnInit, SimpleChanges } from '@angular/core';
import { ActivatedRoute } from "@angular/router";
import { interval, Observable, Subject } from 'rxjs';
import { map, take, takeUntil } from 'rxjs/operators';
import * as moment from "moment";

import { PaginatedList } from '../../../../interfaces/pagination.interfaces';
import { SubmissionInfoDto } from '../../../../interfaces/submission.interfaces';
import { VerdictStage } from '../../../../consts/verdicts.consts';
import { SubmissionService } from '../../../services/submission.service';
import { AuthorizeService } from '../../../../api-authorization/authorize.service';
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

  public contestId: number;
  public contest: ContestViewDto;
  public begun: boolean = true;

  public userId: Observable<string>;
  public pageIndex: number = 1;
  public totalItems: number = 0;
  public totalPages: number = 1;
  public list: PaginatedList<SubmissionInfoDto>;
  public submissions: SubmissionInfoDto[] = [];
  private destroy$ = new Subject();

  constructor(
    private route: ActivatedRoute,
    private auth: AuthorizeService,
    private submissionService: SubmissionService,
    private contestService: ContestService
  ) {
    this.contestId = this.route.snapshot.parent.params.contestId;
    this.userId = this.auth.getUser().pipe(take(1), map(u => u && u.sub));
  }

  ngOnInit() {
    this.contestService.getSingle(this.contestId, true)
      .subscribe(contest => {
        this.contest = contest;
        this.begun = moment().isAfter(this.contest.beginTime);
      });
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
    this.submissionService.newSubmission
      .pipe(takeUntil(this.destroy$))
      .subscribe(submission => {
        this.submissions.unshift(submission);
        if (this.submissions.length > 5) {
          this.submissions.pop();
          this.totalItems++;
          this.totalPages = (this.totalItems + 4) / 5;
        }
      });
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes.problemId) {
      this.pageIndex = 1;
      this.totalItems = 0;
      this.totalPages = 1;
      this.submissions = null;
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

  private loadSubmissions(problemId: number): void {
    this.userId.pipe(take(1)).subscribe(userId => {
      this.submissionService.getPaginatedList(null, userId, null, problemId, null, 5, this.pageIndex)
        .subscribe(list => {
          this.totalItems = list.totalItems;
          this.totalPages = list.totalPages;
          this.submissions = list.items;
        });
    });
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
          }
        }
      });
  }

  public viewSubmissionPopup(submission: SubmissionInfoDto): void {
    window.open('/submission/' + submission.id, '', 'width=930,height=690');
  }
}
