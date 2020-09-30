import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import { PaginatedList } from '../../../../interfaces/pagination.interfaces';
import { BulletinInfoDto } from '../../../../interfaces/bulletin.interfaces';
import { AdminBulletinService } from '../../../services/bulletin.service';
import { faPlus } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-admin-bulletin-list',
  templateUrl: './list.component.html',
  styleUrls: ['./list.component.css']
})
export class AdminBulletinListComponent implements OnInit {
  faPlus = faPlus;

  public pageIndex: number;
  public list: PaginatedList<BulletinInfoDto>;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private service: AdminBulletinService
  ) {
    this.pageIndex = this.route.snapshot.queryParams.pageIndex;
  }

  ngOnInit() {
    this.loadBulletins();
  }

  public loadBulletins() {
    this.service.getPaginatedList(this.pageIndex ?? 1)
      .subscribe(list => this.list = list);
  }

  public onPageIndexChange(value: number) {
    this.pageIndex = value;
    this.router.navigate(['/admin/bulletin'], {
      queryParams: {
        pageIndex: value
      }
    });
    this.loadBulletins();
  }
}
