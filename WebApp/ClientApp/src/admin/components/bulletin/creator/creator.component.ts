import { Component } from '@angular/core';
import { Router } from '@angular/router';

import { AdminBulletinService } from '../../../services/bulletin.service';
import { BulletinEditDto } from '../../../../app/interfaces/bulletin.interfaces';

@Component({
  selector: 'app-admin-bulletin-creator',
  templateUrl: './creator.component.html',
  styleUrls: ['./creator.component.css']
})
export class AdminBulletinCreatorComponent {
  constructor(
    private router: Router,
    private service: AdminBulletinService
  ) {
  }

  public createBulletin(bulletin: BulletinEditDto) {
    this.service.createSingle(bulletin)
      .subscribe(() => {
        this.router.navigate(['/admin/bulletin']);
      }, error => console.error(error));
  }
}
