import { Component } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { AuthorizeService } from '../api-authorization/authorize.service';

@Component({
  selector: 'app-admin-root',
  templateUrl: './admin.component.html',
  styleUrls: ['./admin.component.css']
})
export class AdminComponent {
  public username: Observable<string>;

  constructor(
    private title: Title,
    private auth: AuthorizeService
  ) {
    this.title.setTitle('Administration');
    this.username = this.auth.getUser().pipe(map(u => u && u.name));
  }
}
