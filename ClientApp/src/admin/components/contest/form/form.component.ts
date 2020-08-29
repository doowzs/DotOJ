import { Component } from '@angular/core';

import { AdminContestService } from '../../../services/contest.service';

@Component({
  selector: 'app-admin-contest-form',
  templateUrl: './form.component.html',
  styleUrls: ['./form.component.css']
})
export class AdminContestFormComponent {
  constructor(private service: AdminContestService) {
  }
}
