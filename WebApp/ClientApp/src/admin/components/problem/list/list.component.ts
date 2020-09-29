import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import { PaginatedList } from '../../../../interfaces/pagination.interfaces';
import { ProblemInfoDto } from '../../../../interfaces/problem.interfaces';
import { AdminProblemService } from '../../../services/problem.service';

@Component({
  selector: 'app-admin-problem-list',
  templateUrl: './list.component.html',
  styleUrls: ['./list.component.css']
})
export class AdminProblemListComponent implements OnInit {
  public pageIndex: number;
  public list: PaginatedList<ProblemInfoDto>;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private service: AdminProblemService
  ) {
    this.pageIndex = this.route.snapshot.queryParams.pageIndex;
  }

  ngOnInit() {
    this.loadProblems();
  }

  public loadProblems() {
    this.service.getPaginatedList(this.pageIndex ?? 1)
      .subscribe(list => this.list = list);
  }

  public onPageIndexChange(value: number) {
    this.pageIndex = value;
    this.router.navigate(['/admin/problem'], {
      queryParams: {
        pageIndex: value
      }
    });
    this.loadProblems();
  }
}
