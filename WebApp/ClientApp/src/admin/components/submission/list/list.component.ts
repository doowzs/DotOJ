import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import { PaginatedList } from '../../../../app/interfaces/pagination.interfaces';
import { SubmissionInfoDto } from '../../../../app/interfaces/submission.interfaces';
import { AdminSubmissionService } from '../../../services/submission.service';

@Component({
  selector: 'app-admin-submission-list',
  templateUrl: './list.component.html',
  styleUrls: ['./list.component.css']
})
export class AdminSubmissionListComponent implements OnInit {
  public pageIndex: number;
  public list: PaginatedList<SubmissionInfoDto>;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private service: AdminSubmissionService
  ) {
    this.pageIndex = this.route.snapshot.queryParams.pageIndex;
  }

  ngOnInit() {
    this.loadSubmissions();
  }

  public loadSubmissions() {
    this.service.getPaginatedList(this.pageIndex ?? 1)
      .subscribe(list => this.list = list);
  }

  public onPageIndexChange(value: number) {
    this.pageIndex = value;
    this.router.navigate(['/admin/submission'], {
      queryParams: {
        pageIndex: value
      }
    });
    this.loadSubmissions();
  }
}
