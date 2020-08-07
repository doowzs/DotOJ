import {Component, Input, OnInit, OnChanges, OnDestroy, SimpleChanges} from '@angular/core';
import {forkJoin, interval, Observable, of, Subject, timer} from 'rxjs';
import {delay, takeUntil} from 'rxjs/operators';
import {DateTime} from 'luxon';

import {
  AssignmentViewDto,
  ProblemViewDto,
  SubmissionInfoDto, SubmissionViewDto
} from '../../app.interfaces';
import {SubmissionService} from '../submission.service';

@Component({
  selector: 'app-problem-submissions',
  templateUrl: './problem.component.html'
})
export class ProblemSubmissionsComponent implements OnInit, OnChanges, OnDestroy {
  @Input() public assignment: AssignmentViewDto;
  @Input() public problem: ProblemViewDto;
  public newSubmissions: SubmissionInfoDto[] = [];
  public practiceSubmissions: SubmissionInfoDto[] = [];
  public assignmentSubmissions: SubmissionInfoDto[] = [];
  private updatingNewSubmissions = false;
  private ngUnsubscribe$ = new Subject();

  constructor(
    private service: SubmissionService
  ) {
  }

  ngOnInit() {
    this.service.newSubmission.subscribe(submission => {
      this.newSubmissions.unshift(submission);
      if (!this.updatingNewSubmissions) {
        this.updatingNewSubmissions = true;
        this.updateNewSubmissions();
      }
    });
  }

  ngOnChanges(changes: SimpleChanges) {
    this.service.getListByProblem(changes.problem.currentValue.id)
      .subscribe(submissions => {
        const deadline = this.assignment ? DateTime.fromISO(this.assignment.endTime) : null;
        for (let i = 0; i < submissions.length; ++i) {
          const submission = submissions[i];
          if (deadline && DateTime.fromISO(submission.createdAt) <= deadline) {
            this.assignmentSubmissions.unshift(submission);
          } else {
            this.practiceSubmissions.unshift(submission);
          }
        }
      }, error => console.error(error));
  }

  ngOnDestroy() {
    this.ngUnsubscribe$.next();
    this.ngUnsubscribe$.complete();
  }

  public updateNewSubmissions() {
    const updateObservables: Observable<SubmissionViewDto>[] = [];
    for (let i = 0; i < this.newSubmissions.length; ++i) {
      updateObservables.push(this.service.getSingle(this.newSubmissions[i].id, true));
    }
    const allUpdated$ = updateObservables.length ? forkJoin(updateObservables) : of([]);
    allUpdated$.subscribe(updatedSubmissions => {
      for (let i = 0; i < updatedSubmissions.length; ++i) {
        this.newSubmissions[i].verdict = updatedSubmissions[i].verdict;
        this.newSubmissions[i].lastTestCase = updatedSubmissions[i].lastTestCase;
      }
      of().pipe(delay(2000), takeUntil(this.ngUnsubscribe$))
        .subscribe(() => this.updateNewSubmissions());
    });
  }
}
