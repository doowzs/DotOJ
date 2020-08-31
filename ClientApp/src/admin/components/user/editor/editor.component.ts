import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import { UserEditDto } from '../../../../app/interfaces/user.interfaces';
import { AdminUserService } from '../../../services/user.service';


@Component({
  selector: 'app-admin-user-editor',
  templateUrl: './editor.component.html',
  styleUrls: ['./editor.component.css']
})
export class AdminUserEditorComponent implements OnInit {
  public edit: boolean;
  public userId: string;
  public user: UserEditDto;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private service: AdminUserService
  ) {
    this.edit = this.route.snapshot.queryParams.edit ?? false;
    this.userId = this.route.snapshot.params.userId;
  }

  ngOnInit() {
    this.service.getSingle(this.userId)
      .subscribe(user => this.user = user);
  }

  public editProblem() {
    this.edit = true;
    this.router.navigate(['/admin/user', this.userId], {
      queryParams: { edit: true }
    });
  }

  public updateUser(user: UserEditDto) {
    this.service.updateSingle(this.userId, user).subscribe(() => {
      this.edit = false;
      this.router.navigate(['/admin/user', this.userId]);
    }, error => console.error(error));
  }

  public deleteUser() {
    this.service.deleteSingle(this.userId).subscribe(() => {
      this.router.navigate(['/admin/user']);
    }, error => console.error(error));
  }
}
