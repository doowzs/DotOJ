import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import * as moment from 'moment';

import { ApplicationConfigService } from 'src/app/services/config.service';
import { ContestViewDto } from '../../../../interfaces/contest.interfaces';
import { ContestService } from '../../../services/contest.service';

@Component({
  selector: 'app-contest-view',
  templateUrl: './view.component.html',
  styleUrls: ['./view.component.css']
})
export class ContestViewComponent implements OnInit {
  public contestId: number;
  public contest: ContestViewDto;
  public left: string;
  public ended: boolean;
  public progress: number;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private service: ContestService,
    private config: ApplicationConfigService
  ) {
    this.contestId = this.route.snapshot.params.contestId;
    if (!!config.examId && Number(this.contestId) !== config.examId) {
      this.router.navigateByUrl('/', { skipLocationChange: true })
        .then(() => this.router.navigate(['/contest', config.examId]));
    }
  }

  ngOnInit() {
    this.loadContest();
  }

  public loadContest() {
    this.service.getSingle(this.contestId, true)
      .subscribe(contest => {
        this.contest = contest;
        this.ended = moment().isAfter(this.contest.endTime);
      });
  }
}
