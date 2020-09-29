import { Component, OnInit, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { interval, Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { NzDrawerRef, NzDrawerService } from 'ng-zorro-antd/drawer';
import * as moment from 'moment';

import { SubmissionService } from '../../../services/submission.service';
import { PaginatedList } from '../../../interfaces/pagination.interfaces';
import { SubmissionInfoDto } from '../../../interfaces/submission.interfaces';
import { ContestService } from '../../../services/contest.service';
import { ContestViewDto } from '../../../interfaces/contest.interfaces';
import { Verdicts, VerdictStage } from '../../../consts/verdicts.consts';
import { SubmissionDetailComponent } from '../detail/detail.component';
import { AuthorizeService, IUser } from '../../../../api-authorization/authorize.service';
import { Title } from '@angular/platform-browser';
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

  public user: IUser;
  public contestId: number | null = null;
  public contest: ContestViewDto;

  public loading = true;
  public contestantId: string = "";
  public problemId: string = "";
  public verdict: string = "";
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
    this.contestantId = this.route.snapshot.queryParams.contestantId;
    this.problemId = this.route.snapshot.queryParams.problemId;
    this.verdict = this.route.snapshot.queryParams.verdict;
    this.pageIndex = this.route.snapshot.queryParams.pageIndex ?? 1;
  }

  ngOnInit() {
    this.auth.getUser().subscribe(user => this.user = user);
    this.contestService.getSingle(this.contestId)
      .subscribe(contest => {
        this.contest = contest;
        this.title.setTitle(contest.title + ' - Submissions');
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
    this.loading = true;
    const problemId = this.problemId === '' ? null : Number(this.problemId);
    const verdict = this.verdict === '' ? null : Number(this.verdict);
    this.service.getPaginatedList(this.contestId, null, this.contestantId, problemId, verdict, this.pageIndex)
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
    this.router.navigate(['/contest', this.contestId, 'submissions'], {
      queryParams: {
        contestantId: this.contestantId,
        problemId: this.problemId,
        verdict: this.verdict,
        pageIndex: this.pageIndex
      }
    });
    this.loadSubmissions();
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
