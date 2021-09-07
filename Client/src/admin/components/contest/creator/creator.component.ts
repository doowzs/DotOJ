import { Component } from '@angular/core';
import { Router } from '@angular/router';

import { ContestEditDto } from '../../../../interfaces/contest.interfaces';
import { AdminContestService } from '../../../services/contest.service';

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

  public (contest: ContestEditDto) {
    this.service.createSingle(contest)
      .subscribe(() => {
        this.router.navigate(['/admin/contest']);
      }, error => console.error(error));
  }
}
