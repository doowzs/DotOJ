import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, Router } from '@angular/router';
import { Observable } from 'rxjs';
import { map, tap } from 'rxjs/operators';

import { AuthorizeService } from '../api-authorization/authorize.service';

@Injectable({
  providedIn: 'root'
})
export class AdminGuard implements CanActivate {
  constructor(
    private authorize: AuthorizeService,
    private router: Router
  ) {
  }

  canActivate(route: ActivatedRouteSnapshot): Observable<boolean> | Promise<boolean> | boolean {
    return this.authorize.getUser()
      .pipe(map(u => {
        if (!!u && !!route.data.roles) {
          if (route.data.roles[0] === '*' && u.roles.length > 0) {
            return true;
          }
          for (let i = 0; i < route.data.roles.length; ++i) {
            const role = route.data.roles[i];
            if (u.roles.indexOf(role) >= 0) {
              return true;
            }
          }
        }
        return false;
      }), tap(authorized => {
        if (!authorized) {
          this.router.navigate(['/']);
        }
      }));
  }
}
