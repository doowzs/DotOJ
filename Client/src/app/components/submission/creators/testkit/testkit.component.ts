import { Component, Input, OnInit, OnChanges, SimpleChanges } from '@angular/core';

import { SubmissionService } from '../../../../services/submission.service';
import { faSyncAlt } from '@fortawesome/free-solid-svg-icons';
import { ProblemViewDto } from "../../../../../interfaces/problem.interfaces";
import { ProblemService } from "../../../../services/problem.service";

@Component({
  selector: 'app-submission-testkit-creator',
  templateUrl: './testkit.component.html',
  styleUrls: ['./testkit.component.css']
})
export class SubmissionTestKitCreatorComponent implements OnInit, OnChanges {
  faSyncAlt = faSyncAlt;

  @Input() public problemId: number;
  public problem: ProblemViewDto;
  public token: string;

  constructor(
    private problemService: ProblemService,
    private submissionService: SubmissionService
  ) {
  }

  ngOnInit() {
    this.loadProblemAndToken();
  }

  ngOnChanges(changes: SimpleChanges) {
    if (!!changes.problemId) {
      this.problem = null;
      this.token = null;
      this.loadProblemAndToken();
    }
  }

  public loadProblemAndToken() {
    this.problemService.getSingle(this.problemId)
      .subscribe(problem => {
        this.problem = problem;
        if (problem.type === 1) {
          this.submissionService.getSubmitToken(this.problemId)
            .subscribe(token => this.token = token);
        }
      });
  }
}
