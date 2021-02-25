import { Component, OnDestroy, OnInit } from '@angular/core';
import { take, takeUntil } from "rxjs/operators";
import * as moment from 'moment';

import { ContestInfoDto } from '../../../../interfaces/contest.interfaces';
import { AuthorizeService } from "../../../../api-authorization/authorize.service";
import { ContestService } from '../../../services/contest.service';
import { faBoxOpen, faClock, faLock, faSignInAlt } from '@fortawesome/free-solid-svg-icons';
import { ApplicationConfigService } from "../../../services/config.service";
import { interval, Subject } from "rxjs";

@Component({
  selector: 'app-welcome-contests',
  templateUrl: './contests.component.html',
  styleUrls: ['./contests.component.css']
})
export class WelcomeContestsComponent implements OnInit, OnDestroy {
  faBoxOpen = faBoxOpen;
  faClock = faClock;
  faLock = faLock;
  faSignInAlt = faSignInAlt;

  public privileged = false;
  public contests: ContestInfoDto[];
  public now: moment.Moment;
  private destroy$ = new Subject();

  constructor(
    private auth: AuthorizeService,
    private service: ContestService,
    private config: ApplicationConfigService
  ) {
  }

  ngOnInit() {
    this.auth.getUser()
      .pipe(take(1))
      .subscribe(user => {
        this.privileged = user &&
          (user.roles.indexOf('Administrator') >= 0 ||
            user.roles.indexOf('ContestManager') >= 0);
      });
    this.service.getCurrentList()
      .subscribe(contests => {
        this.now = moment();
        this.contests = contests;
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
