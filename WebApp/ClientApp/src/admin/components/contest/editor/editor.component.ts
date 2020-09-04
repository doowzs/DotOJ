import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import { AdminContestService } from '../../../services/contest.service';
import { ContestEditDto } from '../../../../app/interfaces/contest.interfaces';

@Component({
  selector: 'app-admin-contest-editor',
  templateUrl: './editor.component.html',
  styleUrls: ['./editor.component.css']
})
export class AdminContestEditorComponent implements OnInit {
  public edit: boolean;
  public contestId: number;
  public contest: ContestEditDto;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private service: AdminContestService
  ) {
    this.edit = this.route.snapshot.queryParams.edit ?? false;
    this.contestId = this.route.snapshot.parent.params.contestId;
  }

  ngOnInit() {
    this.service.getSingle(this.contestId)
      .subscribe(contest => this.contest = contest);
  }

  public editContest() {
    this.edit = true;
    this.router.navigate(['/admin/contest', this.contestId], {
      queryParams: { edit: true }
    });
  }

  public updateContest(contest: ContestEditDto) {
    this.service.updateSingle(contest).subscribe(() => {
      this.edit = false;
      this.router.navigate(['/admin/contest', this.contestId]);
    }, error => console.error(error));
  }

  public deleteContest() {
    this.service.deleteSingle(this.contestId).subscribe(() => {
      this.router.navigate(['/admin/contest']);
    }, error => console.error(error));
  }
}
