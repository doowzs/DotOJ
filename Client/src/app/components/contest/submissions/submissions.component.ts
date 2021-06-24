import { Component } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-contest-submissions',
  templateUrl: './submissions.component.html',
  styleUrls: ['./submissions.component.css']
})
export class ContestSubmissionsComponent {
  public contestId: number | null = null;

  constructor(
    private title: Title,
    private route: ActivatedRoute,
  ) {
    this.title.setTitle('评测情况');
    this.contestId = this.route.snapshot.parent.params.contestId;
  }
}
