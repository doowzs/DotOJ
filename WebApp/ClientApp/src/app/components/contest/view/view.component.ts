import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { ContestViewDto } from '../../../interfaces/contest.interfaces';
import { ContestService } from '../../../services/contest.service';

@Component({
  selector: 'app-contest-view',
  templateUrl: './view.component.html',
  styleUrls: ['./view.component.css']
})
export class ContestViewComponent implements OnInit {
  public contestId: number;
  public contest: ContestViewDto;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private service: ContestService
  ) {
    this.contestId = this.route.snapshot.params.contestId;
  }

  ngOnInit() {
    this.service.getSingle(this.contestId)
      .subscribe(contest => this.contest = contest);
  }
}
