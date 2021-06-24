import { Component, OnInit, OnDestroy, Input, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { interval, Subject } from 'rxjs';
import { take, takeUntil } from 'rxjs/operators';

import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { SubmissionDetailComponent } from '../detail/detail.component';

import { Verdicts, VerdictStage } from '../../../../consts/verdicts.consts';
import { PaginatedList } from '../../../../interfaces/pagination.interfaces';
import { ContestViewDto } from '../../../../interfaces/contest.interfaces';
import { SubmissionInfoDto } from '../../../../interfaces/submission.interfaces';
import { AuthorizeService, IUser } from '../../../../api-authorization/authorize.service';
import { ContestService } from '../../../services/contest.service';
import { SubmissionService } from '../../../services/submission.service';
import { faSearch, faSyncAlt } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-submission-list',
  templateUrl: './list.component.html',
  styleUrls: ['./list.component.css']
})
export class SubmissionListComponent implements OnInit, OnDestroy {
  Verdicts = Verdicts;
  faSearch = faSearch;
  faSyncAlt = faSyncAlt;

  @Input() inline: boolean = false;

  public user: IUser;
  public contestId: number | null = null;
  public contest: ContestViewDto;

  public loading = true;
  public contestantId: string = '';
  public problemId: string = '';
  public verdict: string = '';
  public pageIndex: number;
  public list: PaginatedList<SubmissionInfoDto>;
  private destroy$ = new Subject();

  @ViewChild('submissionDetailModal') submissionDetailModal;

  constructor(
    private title: Title,
    private route: ActivatedRoute,
    private router: Router,
    private service: SubmissionService,
    private contestService: ContestService,
    private auth: AuthorizeService,
    private modal: NgbModal
  ) {
    this.contestId = this.route.snapshot.parent.params.contestId;
    this.contestantId = this.route.snapshot.queryParams.contestantId;
    this.problemId = this.route.snapshot.queryParams.problemId;
    this.verdict = this.route.snapshot.queryParams.verdict;
    this.pageIndex = this.route.snapshot.queryParams.pageIndex ?? 1;
    if (!this.contestId && !this.inline) {
      this.title.setTitle('评测情况');
    }
  }

  ngOnInit() {
    this.auth.getUser().pipe(take(1)).subscribe(user => this.user = user);
    if (this.contestId) {
      this.contestService.getSingle(this.contestId)
        .subscribe(contest => {
          this.contest = contest;
          this.title.setTitle(contest.title + ' - 评测情况');
          this.loadSubmissions();
        });
    } else {
      this.loadSubmissions();
    }
    interval(2000)
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        if (this.list && this.list.items.filter(s => {
          return s.verdictInfo.stage === VerdictStage.RUNNING ||
            (s.verdictInfo.stage === VerdictStage.REJECTED && s.score == null);
        }).length > 0) {
          this.updatePendingSubmissions();
        }
      });
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  public getProblemLabel(problemId: number): string {
    if (this.contest) {
      return this.contest.problems.find(p => p.id === problemId).label;
    } else {
      return undefined;
    }
  }

  public loadSubmissions() {
    this.loading = true;
    const contestId = typeof this.contestId === `undefined` ? null : Number(this.contestId);
    const contestantId = this.contestantId ?? null;
    const problemId = typeof this.problemId === `undefined` ? null : Number(this.problemId);
    const verdict = typeof this.verdict === `undefined` ? null : Number(this.verdict);
    this.service.getPaginatedList(contestId, null, contestantId, problemId, verdict, null, this.pageIndex)
      .subscribe(list => {
        this.list = list;
        this.loading = false;
      });
  }

  private updatePendingSubmissions(): void {
    const submissionIds = this.list.items.filter(s => {
      return s.verdictInfo.stage === VerdictStage.RUNNING ||
        (s.verdictInfo.stage === VerdictStage.REJECTED && s.score == null);
    }).map(s => s.id);
    this.service.getBatchInfos(submissionIds)
      .subscribe(updatedSubmissions => {
        for (let i = 0; i < updatedSubmissions.length; ++i) {
          const updated = updatedSubmissions[i];
          const submission = this.list.items.find(s => s.id === updated.id);
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

  public onPageChange(pageIndex: number) {
    this.pageIndex = pageIndex;
    this.onQueryParamsChange();
  }

  public onQueryParamsChange() {
    this.router.navigate(this.inline ? ['/contest', this.contestId, 'submissions'] : ['/submissions'],
      {
        replaceUrl: true,
        queryParams: {
          contestantId: this.contestantId,
          problemId: this.problemId,
          verdict: this.verdict,
          pageIndex: this.pageIndex
        }
      });
    this.loadSubmissions();
  }

  public viewSubmissionPopup(submission: SubmissionInfoDto): void {
    const modelRef = this.modal.open(SubmissionDetailComponent, { size: 'xl' });
    modelRef.componentInstance.submissionId = submission.id;
    modelRef.componentInstance.standalone = false;
  }
}
