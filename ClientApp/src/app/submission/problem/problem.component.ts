import {Component, Input, OnInit, OnChanges, OnDestroy, SimpleChanges} from '@angular/core';
import {forkJoin, interval, Observable, of, Subject, timer} from 'rxjs';
import {take, takeUntil} from 'rxjs/operators';
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
  public updatingNewSubmissions = false;
  private ngUnsubscribe$ = new Subject();

  constructor(
    private service: SubmissionService,
  ) {
  }

  ngOnInit() {
    this.service.newSubmission.subscribe(newSubmission => {
      const deadline = DateTime.fromISO(this.assignment.endTime);
      for (let i = this.newSubmissions.length - 1; i >= 0; --i) {
        const submission = this.newSubmissions[i];
        if (!this.service.isJudging(submission)) {
          if (DateTime.fromISO(submission.createdAt) <= deadline) {
            this.assignmentSubmissions.unshift(submission);
          } else {
            this.practiceSubmissions.unshift(submission);
          }
        }
      }
      this.newSubmissions = this.newSubmissions.filter(s => this.service.isJudging(s));
      this.newSubmissions.unshift(newSubmission);
      if (!this.updatingNewSubmissions) {
        this.updatingNewSubmissions = true;
        this.updateNewSubmissions();
      }
    });
  }

  ngOnChanges(changes: SimpleChanges) {
    this.loadSubmissions(changes.problem.currentValue);
  }

  ngOnDestroy() {
    this.ngUnsubscribe$.next();
    this.ngUnsubscribe$.complete();
  }

  public loadSubmissions(problem: ProblemViewDto) {
    this.service.getListByProblem(problem)
      .subscribe(submissions => {
        this.newSubmissions = [];
        this.practiceSubmissions = [];
        this.assignmentSubmissions = [];
        const deadline = DateTime.fromISO(this.assignment.endTime);
        for (let i = 0; i < submissions.length; ++i) {
          const submission = submissions[i];
          if (this.service.isJudging(submission)) {
            this.newSubmissions.push(submission);
            if (!this.updatingNewSubmissions) {
              this.updatingNewSubmissions = true;
              this.updateNewSubmissions();
            }
          } else if (DateTime.fromISO(submission.createdAt) <= deadline) {
            this.assignmentSubmissions.push(submission);
          } else {
            this.practiceSubmissions.push(submission);
          }
        }
      }, error => console.error(error));
  }

  public updateNewSubmissions() {
    this.updatingNewSubmissions = this.newSubmissions.length > 0;
    const updateObservables: Observable<SubmissionViewDto>[] = [];
    for (let i = 0; i < this.newSubmissions.length; ++i) {
      updateObservables.push(this.service.getSingle(this.newSubmissions[i].id, true));
    }
    forkJoin(updateObservables).subscribe(updatedSubmissions => {
      for (let i = 0; i < updatedSubmissions.length; ++i) {
        this.newSubmissions[i].verdict = updatedSubmissions[i].verdict;
        this.newSubmissions[i].lastTestCase = updatedSubmissions[i].lastTestCase;
      }
      if (this.newSubmissions.filter(s => this.service.isJudging(s)).length) {
        this.updatingNewSubmissions = true;
        interval(2000).pipe(take(1), takeUntil(this.ngUnsubscribe$))
          .subscribe(() => this.updateNewSubmissions());
      } else {
        this.updatingNewSubmissions = false;
      }
    }, error => {
      console.error(error);
      this.updatingNewSubmissions = false;
    });
  }
}
