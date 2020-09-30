import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { UserEditDto } from '../../../../interfaces/user.interfaces';
import { AdminUserService } from '../../../services/user.service';
import { AuthorizeService } from '../../../../api-authorization/authorize.service';
import { faEdit, faTrash } from '@fortawesome/free-solid-svg-icons';


@Component({
  selector: 'app-admin-user-editor',
  templateUrl: './editor.component.html',
  styleUrls: ['./editor.component.css']
})
export class AdminUserEditorComponent implements OnInit {
  faEdit = faEdit;
  faTrash = faTrash;

  public edit: boolean;
  public userId: string;
  public user: UserEditDto;
  public sub: Observable<string>;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private service: AdminUserService,
    private auth: AuthorizeService
  ) {
    this.edit = this.route.snapshot.queryParams.edit ?? false;
    this.userId = this.route.snapshot.params.userId;
    this.sub = this.auth.getUser().pipe(map(u => u && u.sub));
  }

  ngOnInit() {
    this.service.getSingle(this.userId)
      .subscribe(user => this.user = user);
  }

  public editUser() {
    this.edit = true;
    this.router.navigate(['/admin/user', this.userId], {
      replaceUrl: true,
      queryParams: { edit: true }
    });
  }

  public updateUser(user: UserEditDto) {
    this.service.updateSingle(this.userId, user).subscribe(() => {
      this.edit = false;
      this.router.navigate(['/admin/user', this.userId], { replaceUrl: true });
    }, error => console.error(error));
  }

  public deleteUser() {
    if (this.user && confirm('Are you sure to delete user ' + this.user.email + '?')) {
      this.service.deleteSingle(this.userId).subscribe(() => {
        this.router.navigate(['/admin/user']);
      }, error => console.error(error));
    }
  }
}
