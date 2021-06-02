import { Component, OnDestroy, OnInit } from '@angular/core';
import { interval, Subject } from "rxjs";
import { take, takeUntil } from "rxjs/operators";
import * as moment from 'moment';

import { AuthorizeService } from "../../../../api-authorization/authorize.service";
import { ContestInfoDto } from '../../../../interfaces/contest.interfaces';
import { ContestService } from '../../../services/contest.service';
import { ApplicationConfigService } from '../../../services/config.service';
import {
  faClock,
  faLock,
  faSignInAlt,
} from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-welcome-exam',
  templateUrl: './exam.component.html',
  styleUrls: ['./exam.component.css']
})
export class WelcomeExamComponent implements OnInit, OnDestroy {
  faClock = faClock;
  faLock = faLock;
  faSignInAlt = faSignInAlt;

  public examId: number;
  public examLink: string;
  public loading: boolean = false;
  public contest: ContestInfoDto;
  public anonymous: boolean = true;
  public privileged: boolean = false;
  public now: moment.Moment;
  private destroy$ = new Subject();

  constructor(
    private auth: AuthorizeService,
    private service: ContestService,
    private config: ApplicationConfigService
  ) {
    this.loading = true;
    this.examId = this.config.examId;
    this.examLink = `/contest/${this.examId}`;
  }

  ngOnInit() {
    this.auth.getUser()
      .pipe(take(1))
      .subscribe(user => {
        this.anonymous = !user;
        this.privileged = user &&
          (user.roles.indexOf('Administrator') >= 0 ||
            user.roles.indexOf('ContestManager') >= 0);
      });
    this.service.getCurrentList()
      .subscribe(contests => {
        contests.forEach(c => {
          if (c.id === this.examId) {
            this.contest = c;
          }
        });
        this.loading = false;
      });
    this.now = moment().add(this.config.diff, 'ms');
    interval(1000)
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => this.now.add(1, 's'));
    interval(60000)
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => this.now = moment().add(this.config.diff, 'ms'));
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  public canEnterContest(contest: ContestInfoDto): boolean {
    return this.privileged || ((contest.isPublic || contest.registered) && this.now.isAfter(contest.beginTime));
  }
}