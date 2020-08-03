import {Component, Inject, OnDestroy} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {ActivatedRoute, Router} from '@angular/router';
import {interval, Subscriber, Subscription} from 'rxjs';
import {DateTime} from 'luxon';

import {
  AssignmentViewDto, ProblemViewDto
} from '../app.interfaces';

@Component({
  selector: 'app-assignment-view',
  templateUrl: './assignment-view.component.html'
})
export class AssignmentViewComponent implements OnDestroy {
  public assignment: AssignmentViewDto;
  public countdownValue: number;
  public countdownSubscribe: Subscription;
  public problemColumns = ['label', 'title', 'action'];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private http: HttpClient,
    @Inject('BASE_URL') private baseUrl: string
  ) {
    this.loadAssignment(this.route.snapshot.params.assignmentId);
  }

  ngOnDestroy() {
    if (this.countdownSubscribe) {
      this.countdownSubscribe.unsubscribe();
    }
  }

  private loadAssignment(assignmentId: number) {
    this.http.get<AssignmentViewDto>(this.baseUrl + `api/v1/assignment/${assignmentId}`)
      .subscribe(data => {
        this.assignment = data;
        for (let i = 0; i < this.assignment.problems.length; ++i) {
          this.assignment.problems[i].label = String.fromCharCode('A'.charCodeAt(0) + i);
        }
        this.startCountdown();
      }, error => console.error(error));
  }

  private startCountdown() {
    const begin = DateTime.fromISO(this.assignment.beginTime).toMillis();
    const end = DateTime.fromISO(this.assignment.endTime).toMillis();
    const timer$ = interval(1000);
    this.countdownSubscribe = timer$.subscribe(() => {
      const now = DateTime.local().toMillis();
      this.countdownValue = (now - begin) * 100 / (end - begin);
      if (now >= end) {
        this.countdownSubscribe.unsubscribe();
      }
    });
  }
}
