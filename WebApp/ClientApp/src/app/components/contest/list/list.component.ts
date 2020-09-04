import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import * as moment from 'moment';

import { PaginatedList } from '../../../interfaces/pagination.interfaces';
import { ContestInfoDto } from '../../../interfaces/contest.interfaces';
import { ContestService } from '../../../services/contest.service';

@Component({
  selector: 'app-contest-list',
  templateUrl: './list.component.html',
  styleUrls: ['./list.component.css']
})
export class ContestListComponent implements OnInit {
  public list: PaginatedList<ContestInfoDto>;
  public now: moment.Moment;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private service: ContestService
  ) {
  }

  ngOnInit() {
    this.route.queryParams.subscribe(params => {
      this.loadContests(params.pageIndex ?? 1);
    });
  }

  public onPageIndexChange(pageIndex: number) {
    this.router.navigate(['/contests'], {
      queryParams: { pageIndex: pageIndex }
    });
    this.loadContests(pageIndex);
  }

  private loadContests(pageIndex: number): void {
    this.now = moment();
    this.service.getPaginatedList(pageIndex).subscribe(list => this.list = list);
  }

  public isContestRunning(contest: ContestInfoDto): boolean {
    return this.now >= contest.beginTime && this.now <= contest.endTime;
  }

  public canEnterContest(contest: ContestInfoDto): boolean {
    return (this.now >= contest.beginTime && (contest.isPublic || contest.registered)) || this.now > contest.endTime;
  }
}
