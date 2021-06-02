import { Component } from '@angular/core';
import { Observable } from 'rxjs';
import { map, take } from 'rxjs/operators';

import { ApplicationConfigService } from '../../../services/config.service';
import { AuthorizeService } from '../../../../api-authorization/authorize.service';
import {
  faBars,
  faCalendar,
  faCog,
  faHome, faPaperPlane,
  faSignInAlt,
  faSignOutAlt, faTools,
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
  faPaperPlane = faPaperPlane;
  faSignInAlt = faSignInAlt;
  faSignOutAlt = faSignOutAlt;
  faTools = faTools;
  faUser = faUser;
  faUserPlus = faUserPlus;

  public title: string;
  public isExamMode: boolean = false;
  public username: Observable<string>;
  public collapse = true;
  public isAuthenticated: Observable<boolean>;
  public canViewAdminPages: Observable<boolean>;

  constructor(
    private auth: AuthorizeService,
    private config: ApplicationConfigService
  ) {
    this.title = config.title;
    this.isExamMode = !!config.examId;
    this.username = this.auth.getUser().pipe(take(1), map(u => u && u.name));
    this.isAuthenticated = this.auth.isAuthenticated();
    this.canViewAdminPages = this.auth.getUser().pipe(take(1), map(u => u && u.roles.length > 0));
  }
}
