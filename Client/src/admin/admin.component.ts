import { Component, OnInit } from '@angular/core';
import { Title } from '@angular/platform-browser';

import { AuthorizeService, IUser } from '../auth/authorize.service';
import {
  faBars,
  faCalendar,
  faCog,
  faHome, faPaperPlane,
  faSignInAlt,
  faSignOutAlt,
  faUser,
  faUserPlus
} from '@fortawesome/free-solid-svg-icons';
import { take } from 'rxjs/operators';

@Component({
  selector: 'app-admin-root',
  templateUrl: './admin.component.html',
  styleUrls: ['./admin.component.css']
})
export class AdminComponent implements OnInit {
  faBars = faBars;
  faCalendar = faCalendar;
  faCog = faCog;
  faHome = faHome;
  faPaperPlane = faPaperPlane;
  faSignInAlt = faSignInAlt;
  faSignOutAlt = faSignOutAlt;
  faUser = faUser;
  faUserPlus = faUserPlus;

  public user: IUser;
  public collapse = true;

  constructor(
    public title: Title,
    private auth: AuthorizeService
  ) {
    this.title.setTitle('Administration');
  }

  ngOnInit() {
    this.auth.getUser().pipe(take(1)).subscribe(u => this.user = u);
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
