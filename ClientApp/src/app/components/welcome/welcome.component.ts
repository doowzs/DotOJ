import { Component, OnInit } from '@angular/core';

import { AssignmentService } from '../../services/assignment.service';
import { ApplicationConfigService } from '../../services/config.service';
import { AssignmentInfoDto } from '../../interfaces/assignment.interfaces';

@Component({
  selector: 'app-welcome',
  templateUrl: './welcome.component.html'
})
export class WelcomeComponent implements OnInit {
  public assignments: AssignmentInfoDto[];

  constructor(
    private service: AssignmentService,
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
