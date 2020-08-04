import {Component, OnInit} from '@angular/core';
import {ActivatedRoute, Router} from '@angular/router';
import {ProblemViewDto} from '../../app.interfaces';
import {ProblemService} from '../problem.service';

@Component({
  selector: 'app-problem-content',
  templateUrl: './content.component.html'
})
export class ProblemContentComponent implements OnInit {
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
      });
  }
}
