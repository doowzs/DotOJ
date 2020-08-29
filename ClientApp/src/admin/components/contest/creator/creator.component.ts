import { Component } from '@angular/core';

import { AdminContestService } from '../../../services/contest.service';

@Component({
  selector: 'app-admin-contest-creator',
  templateUrl: './creator.component.html',
  styleUrls: ['./creator.component.css']
})
export class AdminContestCreatorComponent {
  constructor(private service: AdminContestService) {
  }
}
