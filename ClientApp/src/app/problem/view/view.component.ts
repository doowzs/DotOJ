import {Component, OnInit} from '@angular/core';
import {ActivatedRoute, Router} from '@angular/router';

import {ProblemViewDto} from '../../app.interfaces';
import {ProblemService} from '../problem.service';

@Component({
  selector: 'app-problem-view',
  templateUrl: './view.component.html'
})
export class ProblemViewComponent implements OnInit {
  public id: number;
  public problem: ProblemViewDto;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private service: ProblemService
  ) {
  }

  ngOnInit() {
    this.id = this.route.snapshot.params.problemId;
    this.service.getSingle(this.id)
      .subscribe(problem => {
        this.problem = problem;
      }, error => console.error(error));
  }

  // TODO: implement problem view component
}
