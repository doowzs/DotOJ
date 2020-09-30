import { Component } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-admin-contest-view',
  templateUrl: './view.component.html',
  styleUrls: ['./view.component.css']
})
export class AdminContestViewComponent {
  public contestId: number;

  constructor(
    private route: ActivatedRoute
  ) {
    this.contestId = this.route.snapshot.params.contestId;
  }
}
