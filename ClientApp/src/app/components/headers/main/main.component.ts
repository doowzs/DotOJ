import { Component } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { ApplicationConfigService } from '../../../services/config.service';
import { AuthorizeService } from '../../../../api-authorization/authorize.service';

@Component({
  selector: 'app-header-main',
  templateUrl: './main.component.html',
  styleUrls: ['./main.component.css']
})
export class MainHeaderComponent {
  public title: string;
  public username: Observable<string>;
  public isAuthenticated: Observable<boolean>;

  constructor(
    private auth: AuthorizeService,
    private config: ApplicationConfigService
  ) {
    this.title = config.title;
    this.username = this.auth.getUser().pipe(map(u => u && u.name));
    this.isAuthenticated = this.auth.isAuthenticated();
  }
}
