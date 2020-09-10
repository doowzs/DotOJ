import { Component, OnInit, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { interval, Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { NzTableFilterList, NzTableQueryParams } from 'ng-zorro-antd/table';
import { NzDrawerRef, NzDrawerService } from 'ng-zorro-antd/drawer';
import * as moment from 'moment';

import { SubmissionService } from '../../../services/submission.service';
import { PaginatedList } from '../../../interfaces/pagination.interfaces';
import { SubmissionInfoDto } from '../../../interfaces/submission.interfaces';
import { ContestService } from '../../../services/contest.service';
import { ContestViewDto } from '../../../interfaces/contest.interfaces';
import { VerdictInfo, Verdicts, VerdictStage } from '../../../consts/verdicts.consts';
import { SubmissionDetailComponent } from '../detail/detail.component';
import { AuthorizeService, IUser } from '../../../../api-authorization/authorize.service';
import { Title } from '@angular/platform-browser';

@Component({
  selector: 'app-submission-list',
  templateUrl: './list.component.html',
  styleUrls: ['./list.component.css']
})
export class SubmissionListComponent implements OnInit, OnDestroy {
  Verdicts = Verdicts;

  public user: IUser;
  public contestId: number | null = null;
  public contest: ContestViewDto;
  public problemFilterList: NzTableFilterList;
  public verdictFilterList: NzTableFilterList;

  public problemId: number | null = null;
  public userId: string | null = null;
  public verdict: number | null = null;
  public pageIndex: number;
  public list: PaginatedList<SubmissionInfoDto>;
  public submissionDrawer: NzDrawerRef;
  private destroy$ = new Subject();

  constructor(
    private title: Title,
    private route: ActivatedRoute,
    private router: Router,
    private service: SubmissionService,
    private contestService: ContestService,
    private drawer: NzDrawerService,
    private auth: AuthorizeService
  ) {
    this.contestId = this.route.snapshot.parent.params.contestId;
    this.problemId = this.route.snapshot.queryParams.problemId;
    this.userId = this.route.snapshot.queryParams.userId;
    this.verdict = this.route.snapshot.queryParams.verdict;
    this.pageIndex = this.route.snapshot.queryParams.pageIndex ?? 1;

    this.verdictFilterList = [];
    for (let i = 0; i < Verdicts.length; ++i) {
      const verdict = Verdicts[i];
      this.verdictFilterList.push({
        text: verdict.name,
        value: verdict.code,
        byDefault: verdict.code === Number(this.verdict)
      });
    }
  }

  ngOnInit() {
    this.auth.getUser().subscribe(user => this.user = user);
    this.contestService.getSingle(this.contestId)
      .subscribe(contest => {
        this.contest = contest;
        this.title.setTitle(contest.title + ' - Submissions');
        this.problemFilterList = [];
        for (let i = 0; i < contest.problems.length; ++i) {
          const problem = contest.problems[i];
          this.problemFilterList.push({
            text: this.getProblemLabel(problem.id) + ': ' + problem.title,
            value: problem.id,
            byDefault: problem.id === Number(this.problemId)
          });
        }
        this.loadSubmissions();
      });
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
    return this.contest.problems.find(p => p.id === problemId).label;
  }

  public loadSubmissions() {
    this.service.getPaginatedList(this.contestId, this.problemId, this.userId, this.verdict, this.pageIndex)
      .subscribe(list => {console.log(list); this.list = list});
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

  public onQueryParamsChange(params: NzTableQueryParams) {
    const isInit = this.route.snapshot.queryParams.pageIndex == null;
    this.problemId = params.filter.find(f => f.key === 'problemId').value;
    this.verdict = params.filter.find(f => f.key === 'verdict').value;
    this.pageIndex = params.pageIndex;
    this.router.navigate(['/contest', this.contestId, 'submissions'], {
      queryParams: {
        problemId: this.problemId,
        userId: this.userId,
        verdict: this.verdict,
        pageIndex: this.pageIndex
      }
    });
    if (!isInit) {
      this.loadSubmissions();
    }
  }

  public canViewSubmission(submission: SubmissionInfoDto): boolean {
    return (moment().isAfter(this.contest.endTime)) || (this.user && submission.userId === this.user.sub);
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
