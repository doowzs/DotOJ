import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import { PaginatedList } from '../../../../interfaces/pagination.interfaces';
import { UserInfoDto } from '../../../../interfaces/user.interfaces';
import { AdminUserService } from '../../../services/user.service';
import { faPlus } from "@fortawesome/free-solid-svg-icons";

@Component({
  selector: 'app-admin-user-list',
  templateUrl: './list.component.html',
  styleUrls: ['./list.component.css']
})
export class AdminUserListComponent implements OnInit {
  faPlus = faPlus;

  public pageIndex: number;
  public list: PaginatedList<UserInfoDto>;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private service: AdminUserService
  ) {
    this.pageIndex = this.route.snapshot.queryParams.pageIndex;
  }

  ngOnInit() {
    this.loadUsers();
  }

  public loadUsers() {
    this.service.getPaginatedList(this.pageIndex ?? 1)
      .subscribe(list => this.list = list);
  }

  public onPageIndexChange(value: number) {
    this.pageIndex = value;
    this.router.navigate(['/admin/user'], {
      queryParams: {
        pageIndex: value
      }
    });
    this.loadUsers();
  }
}
