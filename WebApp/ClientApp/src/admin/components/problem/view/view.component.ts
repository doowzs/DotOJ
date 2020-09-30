import { Component } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { faArchive, faBoxes, faEdit } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-admin-problem-view',
  templateUrl: './view.component.html',
  styleUrls: ['./view.component.css']
})
export class AdminProblemViewComponent {
  faArchive = faArchive;
  faBoxes = faBoxes;
  faEdit = faEdit;

  public problemId: number;

  constructor(
    private route: ActivatedRoute
  ) {
    this.problemId = this.route.snapshot.params.problemId;
  }
}
