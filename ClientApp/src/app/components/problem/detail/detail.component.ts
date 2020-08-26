import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

import { ProblemService } from '../../../services/problem.service';
import { ProblemViewDto } from '../../../interfaces/problem.interfaces';
import { ContestViewDto } from '../../../interfaces/contest.interfaces';
import { ContestService } from '../../../services/contest.service';

@Component({
  selector: 'app-problem-detail',
  templateUrl: './detail.component.html',
  styleUrls: ['./detail.component.css']
})
export class ProblemDetailComponent implements OnInit {
  public contestId: number;
  public contest: ContestViewDto;

  public problemId: number;
  public problem: ProblemViewDto;

  copyToClipboard = (content: string): void => {
    navigator.clipboard.writeText(content);
  }

  constructor(
    private route: ActivatedRoute,
    private problemService: ProblemService,
    private contestService: ContestService
  ) {
    this.contestId = this.route.snapshot.parent.params.contestId;
    this.problemId = this.route.snapshot.params.problemId;
  }

  ngOnInit() {
    this.contestService.getSingle(this.contestId)
      .subscribe(contest => this.contest = contest);
    this.problemService.getSingle(this.problemId)
      .subscribe(problem => this.problem = problem);
  }
}

