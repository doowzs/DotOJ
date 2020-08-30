﻿import { Component } from '@angular/core';
import { Router } from '@angular/router';

import { AdminContestService } from '../../../services/contest.service';
import { ContestEditDto } from '../../../../app/interfaces/contest.interfaces';

@Component({
  selector: 'app-admin-contest-creator',
  templateUrl: './creator.component.html',
  styleUrls: ['./creator.component.css']
})
export class AdminContestCreatorComponent {
  constructor(
    private router: Router,
    private service: AdminContestService
  ) {
  }

  public createContest(contest: ContestEditDto) {
    this.service.createSingle(contest)
      .subscribe(() => {
        this.router.navigate(['/admin/contest']);
      }, error => console.error(error));
  }
}
