import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ContestMode, ContestViewDto } from '../../../interfaces/contest.interfaces';
import { ContestService } from '../../../services/contest.service';

@Component({
  selector: 'app-contest-description',
  templateUrl: './description.component.html',
  styleUrls: ['./description.component.css']
})
export class ContestDescriptionComponent implements OnInit {
  ContestMode = ContestMode;
  public contestId: number;
  public contest: ContestViewDto;

  constructor(
    private route: ActivatedRoute,
    private service: ContestService
  ) {
    this.contestId = this.route.snapshot.params.contestId;
  }

  ngOnInit() {
    this.service.getSingle(this.contestId)
      .subscribe(contest => this.contest = contest);
  }
}
