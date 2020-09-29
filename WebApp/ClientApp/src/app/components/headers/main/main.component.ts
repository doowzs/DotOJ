import { Component } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { ApplicationConfigService } from '../../../services/config.service';
import { AuthorizeService } from '../../../../api-authorization/authorize.service';
import {
  faBars,
  faCalendar,
  faCog,
  faHome,
  faSignInAlt,
  faSignOutAlt,
  faUser,
  faUserPlus
} from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-header-main',
  templateUrl: './main.component.html',
  styleUrls: ['./main.component.css']
})
export class MainHeaderComponent {
  faBars = faBars;
  faCalendar = faCalendar;
  faCog = faCog;
  faHome = faHome;
  faSignInAlt = faSignInAlt;
  faSignOutAlt = faSignOutAlt;
  faUser = faUser;
  faUserPlus = faUserPlus;

  public title: string;
  public username: Observable<string>;
  public collapse = true;
  public isAuthenticated: Observable<boolean>;
  public canViewAdminPages: Observable<boolean>;

  constructor(
    private auth: AuthorizeService,
    private config: ApplicationConfigService
  ) {
    this.title = config.title;
    this.username = this.auth.getUser().pipe(map(u => u && u.name));
    this.isAuthenticated = this.auth.isAuthenticated();
    this.canViewAdminPages = this.auth.getUser().pipe(map(u => u && u.roles.length > 0));
  }
}
