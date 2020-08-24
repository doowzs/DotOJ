import { Component, OnInit } from '@angular/core';
import * as moment from 'moment';

import { ContestService } from '../../services/contest.service';
import { ApplicationConfigService } from '../../services/config.service';
import { ContestInfoDto } from '../../interfaces/contest.interfaces';

@Component({
  selector: 'app-welcome',
  templateUrl: './welcome.component.html',
  styleUrls: ['./welcome.component.css']
})
export class WelcomeComponent implements OnInit {
  public messageOfTheDay: string;
  public now: moment.Moment;
  public contests: ContestInfoDto[];

  constructor(
    private service: ContestService,
    private config: ApplicationConfigService
  ) {
    this.messageOfTheDay = config.messageOfTheDay;
  }

  ngOnInit() {
    this.service.getUpcomingList()
      .subscribe(contests => {
        this.now = moment();
        this.contests = contests;
      });
  }
}
