import { Component, OnInit } from '@angular/core';
import { take } from "rxjs/operators";
import * as moment from 'moment';

import { ContestInfoDto } from '../../../../interfaces/contest.interfaces';
import { AuthorizeService } from "../../../../api-authorization/authorize.service";
import { ContestService } from '../../../services/contest.service';
import { faBoxOpen, faClock, faLock, faSignInAlt } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-welcome-contests',
  templateUrl: './contests.component.html',
  styleUrls: ['./contests.component.css']
})
export class WelcomeContestsComponent implements OnInit {
  faBoxOpen = faBoxOpen;
  faClock = faClock;
  faLock = faLock;
  faSignInAlt = faSignInAlt;

  public privileged = false;
  public now: moment.Moment;
  public contests: ContestInfoDto[];

  constructor(
    private auth: AuthorizeService,
    private service: ContestService
  ) {
  }

  ngOnInit() {
    this.auth.getUser()
      .pipe(take(1))
      .subscribe(user => {
        this.privileged = user.roles.indexOf('Administrator') >= 0
          || user.roles.indexOf('ContestManager') >= 0;
      });
    this.service.getCurrentList()
      .subscribe(contests => {
        this.now = moment();
        this.contests = contests;
      });
  }

  public canEnterContest(contest: ContestInfoDto): boolean {
    return this.privileged || ((contest.isPublic || contest.registered) && moment().isAfter(contest.beginTime));
  }
}
