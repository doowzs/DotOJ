import { Component } from '@angular/core';

import { AdminContestService } from '../../../services/contest.service';

@Component({
  selector: 'app-admin-contest-list',
  templateUrl: './list.component.html',
  styleUrls: ['./list.component.css']
})
export class AdminContestListComponent {
  constructor(private service: AdminContestService) {
  }
}
