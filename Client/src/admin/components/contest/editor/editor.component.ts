﻿import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import { ContestEditDto } from '../../../../interfaces/contest.interfaces';
import { AdminContestService } from '../../../services/contest.service';
import { faEdit, faTrash } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-admin-contest-editor',
  templateUrl: './editor.component.html',
  styleUrls: ['./editor.component.css']
})
export class AdminContestEditorComponent implements OnInit {
  faEdit = faEdit;
  faTrash = faTrash;

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
      replaceUrl: true,
      queryParams: { edit: true }
    });
  }

  public updateContest(contest: ContestEditDto) {
    this.service.updateSingle(contest).subscribe(() => {
      this.edit = false;
      this.router.navigate(['/admin/contest', this.contestId], { replaceUrl: true });
    }, error => console.error(error));
  }

  public deleteContest() {
    if (confirm('Are you sure you want to delete contest #' + this.contestId + '?')) {
      this.service.deleteSingle(this.contestId).subscribe(() => {
        this.router.navigate(['/admin/contest']);
      }, error => console.error(error));
    }
  }
}
