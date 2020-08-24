import { Component, OnInit } from '@angular/core';

import { ContestService } from '../../services/contest.service';
import { ApplicationConfigService } from '../../services/config.service';
import { ContestInfoDto } from '../../interfaces/assignment.interfaces';

@Component({
  selector: 'app-welcome',
  templateUrl: './welcome.component.html'
})
export class WelcomeComponent implements OnInit {
  public assignments: ContestInfoDto[];

  constructor(
    private service: ContestService,
    private config: ApplicationConfigService
  ) {
  }

  ngOnInit() {
    this.service.getOngoingList()
      .subscribe(assignments => {
        this.assignments = assignments;
      });
  }
}
