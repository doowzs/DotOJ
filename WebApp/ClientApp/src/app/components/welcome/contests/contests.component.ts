import { Component, OnInit } from '@angular/core';
import * as moment from 'moment';

import { ContestInfoDto } from '../../../../interfaces/contest.interfaces';
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

  public now: moment.Moment;
  public contests: ContestInfoDto[];

  constructor(private service: ContestService) {
  }

  ngOnInit() {
    this.service.getCurrentList()
      .subscribe(contests => {
        this.now = moment();
        this.contests = contests;
      });
  }

  public canEnterContest(contest: ContestInfoDto): boolean {
    return (contest.isPublic || contest.registered) && moment().isAfter(contest.beginTime);
  }
}
