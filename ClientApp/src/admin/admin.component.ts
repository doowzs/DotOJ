import {Component} from '@angular/core';
import {Observable} from 'rxjs';
import {map} from 'rxjs/operators';

import {AuthorizeService} from '../api-authorization/authorize.service';

@Component({
  selector: 'app-admin',
  templateUrl: './admin.component.html'
})
export class AdminComponent {
  public username: Observable<string>;

  constructor(private service: AuthorizeService) {
    this.username = service.getUser().pipe(map(u => u && u.name));
  }
}
