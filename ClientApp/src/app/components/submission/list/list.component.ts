import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { NzTableFilterList, NzTableQueryParams } from 'ng-zorro-antd/table';

import { SubmissionService } from '../../../services/submission.service';
import { PaginatedList } from '../../../interfaces/pagination.interfaces';
import { SubmissionInfoDto } from '../../../interfaces/submission.interfaces';
import { ContestService } from '../../../services/contest.service';
import { ContestViewDto } from '../../../interfaces/contest.interfaces';
import { Verdicts } from '../../../consts/verdicts.consts';

@Component({
  selector: 'app-submission-list',
  templateUrl: './list.component.html',
  styleUrls: ['./list.component.css']
})
export class SubmissionListComponent implements OnInit {
  Verdicts = Verdicts;

  public contestId: number | null = null;
  public contest: ContestViewDto;
  public problemFilterList: NzTableFilterList;
  public verdictFilterList: NzTableFilterList;

  public problemId: number | null = null;
  public userId: string | null = null;
  public verdict: number | null = null;
  public pageIndex: number;
  public list: PaginatedList<SubmissionInfoDto>;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private service: SubmissionService,
    private contestService: ContestService
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
    this.contestService.getSingle(this.contestId)
      .subscribe(contest => {
        this.contest = contest;
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
  }

  public getProblemLabel(problemId: number): string {
    return this.contest.problems.find(p => p.id === problemId).label;
  }

  public loadSubmissions() {
    this.service.getPaginatedList(this.contestId, this.problemId, this.userId, this.verdict, this.pageIndex)
      .subscribe(list => this.list = list);
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
}
