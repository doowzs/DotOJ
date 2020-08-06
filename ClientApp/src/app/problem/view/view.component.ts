import {Component, OnInit, OnDestroy} from '@angular/core';
import {ActivatedRoute, Router} from '@angular/router';
import {BreakpointObserver} from '@angular/cdk/layout';
import {Subject} from 'rxjs';
import {takeUntil} from 'rxjs/operators';

import {AssignmentViewDto, ProblemViewDto} from '../../app.interfaces';
import {AssignmentService} from '../../assignment/assignment.service';
import {ProblemService} from '../problem.service';

@Component({
  selector: 'app-problem-view',
  templateUrl: './view.component.html'
})
export class ProblemViewComponent implements OnInit, OnDestroy {
  private assignmentId: number;
  public assignment: AssignmentViewDto;
  private problemId: number;
  public problem: ProblemViewDto;
  public showCodeEditor: boolean;
  private ngUnsubscribe$ = new Subject();

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private assignmentService: AssignmentService,
    private problemService: ProblemService,
    private breakpointObserver: BreakpointObserver
  ) {
  }

  ngOnInit() {
    this.route.parent.params.subscribe(params => {
      this.assignmentId = params.assignmentId;
      this.assignmentService.getSingle(this.assignmentId)
        .subscribe(assignment => {
          this.assignment = assignment;
        }, error => console.error(error));
    });

    this.route.params.subscribe(params => {
      this.problemId = params.problemId;
      this.problemService.getSingle(this.problemId)
        .subscribe(problem => {
          this.problem = problem;
        }, error => console.error(error));
    });

    this.breakpointObserver
      .observe(['(max-width: 999px)'])
      .pipe(takeUntil(this.ngUnsubscribe$))
      .subscribe(result => {
        this.showCodeEditor = !result.matches;
      });
  }

  ngOnDestroy() {
    this.ngUnsubscribe$.next();
    this.ngUnsubscribe$.complete();
  }
}
