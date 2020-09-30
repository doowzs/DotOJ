import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import { PaginatedList } from '../../../../interfaces/pagination.interfaces';
import { ContestInfoDto, ContestMode } from '../../../../interfaces/contest.interfaces';
import { AdminContestService } from '../../../services/contest.service';
import { faPlus } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-admin-contest-list',
  templateUrl: './list.component.html',
  styleUrls: ['./list.component.css']
})
export class AdminContestListComponent implements OnInit {
  faPlus = faPlus;
  ContestMode = ContestMode;

  public pageIndex: number;
  public list: PaginatedList<ContestInfoDto>;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private service: AdminContestService
  ) {
    this.pageIndex = this.route.snapshot.queryParams.pageIndex;
  }

  ngOnInit() {
    this.loadContests();
  }

  public loadContests() {
    this.service.getPaginatedList(this.pageIndex ?? 1)
      .subscribe(list => this.list = list);
  }

  public onPageIndexChange(value: number) {
    this.pageIndex = value;
    this.router.navigate(['/admin/contest'], {
      queryParams: {
        pageIndex: value
      }
    });
    this.loadContests();
  }
}
