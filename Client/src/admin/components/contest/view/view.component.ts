import { Component } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { faEdit, faUsers } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-admin-contest-view',
  templateUrl: './view.component.html',
  styleUrls: ['./view.component.css']
})
export class AdminContestViewComponent {
  faEdit = faEdit;
  faUsers = faUsers;

  public contestId: number;

  constructor(
    private route: ActivatedRoute
  ) {
    this.contestId = this.route.snapshot.params.contestId;
  }
}
