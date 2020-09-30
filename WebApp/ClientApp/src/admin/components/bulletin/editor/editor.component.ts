import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import { BulletinEditDto } from '../../../../interfaces/bulletin.interfaces';
import { AdminBulletinService } from '../../../services/bulletin.service';
import { faEdit, faTrash } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-admin-bulletin-editor',
  templateUrl: './editor.component.html',
  styleUrls: ['./editor.component.css']
})
export class AdminBulletinEditorComponent implements OnInit {
  faEdit = faEdit;
  faTrash = faTrash;

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
      replaceUrl: true,
      queryParams: { edit: true }
    });
  }

  public updateBulletin(bulletin: BulletinEditDto) {
    this.service.updateSingle(bulletin).subscribe(() => {
      this.edit = false;
      this.router.navigate(['/admin/bulletin', this.bulletinId], { replaceUrl: true });
    }, error => console.error(error));
  }

  public deleteBulletin() {
    if (confirm('Are you sure to delete bulletin #' + this.bulletinId + '?')) {
      this.service.deleteSingle(this.bulletinId).subscribe(() => {
        this.router.navigate(['/admin/bulletin']);
      }, error => console.error(error));
    }
  }
}
