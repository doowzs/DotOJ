import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

import { ProblemService } from '../../../services/problem.service';
import { ProblemViewDto } from '../../../interfaces/problem.interfaces';

@Component({
  selector: 'app-problem-detail',
  templateUrl: './detail.component.html',
  styleUrls: ['./detail.component.css']
})
export class ProblemDetailComponent implements OnInit {
  public problemId: number;
  public problem: ProblemViewDto;

  copyToClipboard = (content: string): void => {
    navigator.clipboard.writeText(content);
  }

  constructor(
    private route: ActivatedRoute,
    private service: ProblemService
  ) {
    this.problemId = this.route.snapshot.params.problemId;
  }

  ngOnInit() {
    this.service.getSingle(this.problemId)
      .subscribe(problem => this.problem = problem);
  }
}

