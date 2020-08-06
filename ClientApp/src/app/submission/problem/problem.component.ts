import {Component, Input, OnInit} from '@angular/core';

import {
  ProblemViewDto,
  SubmissionInfoDto
} from '../../app.interfaces';
import {SubmissionService} from '../submission.service';

@Component({
  selector: 'app-problem-submissions',
  templateUrl: './problem.component.html'
})
export class ProblemSubmissionsComponent implements OnInit {
  @Input() public problem: ProblemViewDto;
  public submissions: SubmissionInfoDto[];

  constructor(
    private service: SubmissionService
  ) {
  }

  ngOnInit() {
    this.service.getListByProblem(this.problem.id)
      .subscribe(submissions => {
        this.submissions = submissions;
      }, error => console.error(error));
  }
}
