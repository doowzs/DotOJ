import { Component, OnInit } from '@angular/core';
import * as moment from 'moment';

import { ContestService } from '../../../services/contest.service';
import { ContestInfoDto } from '../../../interfaces/contest.interfaces';

@Component({
  selector: 'app-welcome-contests',
  templateUrl: './contests.component.html',
  styleUrls: ['./contests.component.css']
})
export class WelcomeContestsComponent implements OnInit {
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