﻿import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import { AdminBulletinService } from '../../../services/bulletin.service';
import { BulletinEditDto } from '../../../../app/interfaces/bulletin.interfaces';

@Component({
  selector: 'app-admin-bulletin-editor',
  templateUrl: './editor.component.html',
  styleUrls: ['./editor.component.css']
})
export class AdminBulletinEditorComponent implements OnInit {
  public edit: boolean;
  public bulletinId: number;
  public bulletin: BulletinEditDto;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private service: AdminBulletinService
  ) {
    this.edit = this.route.snapshot.queryParams.edit ?? false;
    this.bulletinId = this.route.snapshot.params.bulletinId;
  }

  ngOnInit() {
    this.service.getSingle(this.bulletinId)
      .subscribe(bulletin => this.bulletin = bulletin);
  }

  public editBulletin() {
    this.edit = true;
    this.router.navigate(['/admin/bulletin', this.bulletinId], {
      queryParams: { edit: true }
    });
  }

  public updateBulletin(bulletin: BulletinEditDto) {
    this.service.updateSingle(bulletin).subscribe(() => {
      this.edit = false;
      this.router.navigate(['/admin/bulletin', this.bulletinId]);
    }, error => console.error(error));
  }

  public deleteBulletin() {
    this.service.deleteSingle(this.bulletinId).subscribe(() => {
      this.router.navigate(['/admin/bulletin']);
    }, error => console.error(error));
  }
}
