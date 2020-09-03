import { Component, OnInit } from '@angular/core';
import { Title } from '@angular/platform-browser';

import { AuthorizeService, IUser } from '../api-authorization/authorize.service';

@Component({
  selector: 'app-admin-root',
  templateUrl: './admin.component.html',
  styleUrls: ['./admin.component.css']
})
export class AdminComponent implements OnInit {
  public user: IUser;

  constructor(
    private title: Title,
    private auth: AuthorizeService
  ) {
    this.title.setTitle('Administration');
  }

  ngOnInit() {
    this.auth.getUser().subscribe(u => this.user = u);
  }

  public hasAnyRole(roles: string[]): boolean {
    for (let i = 0; i < roles.length; ++i) {
      if (this.user.roles.indexOf(roles[i]) >= 0) {
        return true;
      }
    }
    return false;
  }
}
