import {Component, OnInit, OnDestroy} from '@angular/core';
import {ActivatedRoute, Router} from '@angular/router';
import {BreakpointObserver} from '@angular/cdk/layout';
import {Subscription} from 'rxjs';

import {ProblemViewDto} from '../../app.interfaces';
import {ProblemService} from '../problem.service';

@Component({
  selector: 'app-problem-view',
  templateUrl: './view.component.html'
})
export class ProblemViewComponent implements OnInit, OnDestroy {
  public id: number;
  public problem: ProblemViewDto;
  public showCodeEditor: boolean;
  private breakpointSubscription: Subscription;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private service: ProblemService,
    private breakpointObserver: BreakpointObserver
  ) {
  }

  ngOnInit() {
    this.id = this.route.snapshot.params.problemId;
    this.service.getSingle(this.id)
      .subscribe(problem => {
        this.problem = problem;
        console.log(problem);
      }, error => console.error(error));
    this.breakpointSubscription = this.breakpointObserver
      .observe(['(max-width: 599px)'])
      .subscribe(result => {
        console.log(result);
        this.showCodeEditor = !result.matches;
      });
  }

  ngOnDestroy() {
    if (this.breakpointSubscription) {
      this.breakpointSubscription.unsubscribe();
    }
  }

  // TODO: implement problem view component
}
