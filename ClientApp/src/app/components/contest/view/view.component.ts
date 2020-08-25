import { Component } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-contest-view',
  templateUrl: './view.component.html',
  styleUrls: ['./view.component.css']
})
export class ContestViewComponent {
  public contestId: number;

  constructor(
    private route: ActivatedRoute,
    private router: Router
  ) {
    this.contestId = this.route.snapshot.params.contestId;
  }
}
