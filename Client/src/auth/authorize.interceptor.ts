import {Injectable} from '@angular/core';
import {Router} from '@angular/router';
import {HttpInterceptor, HttpRequest, HttpHandler, HttpEvent} from '@angular/common/http';
import {Observable, throwError} from 'rxjs';
import {catchError, mergeMap} from 'rxjs/operators';
import {AuthorizeService} from './authorize.service';

export const SkipInjectTokenHeader = 'X-Skip-Inject-Token';
export const SkipErrorRedirectHeader = 'X-Skip-Error-Redirect';

@Injectable({
  providedIn: 'root'
})
export class AuthorizeInterceptor implements HttpInterceptor {
  constructor(
    private router: Router,
    private authorize: AuthorizeService
  ) {
  }

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    if (req.headers.has(SkipInjectTokenHeader)) {
      const headers = req.headers.delete(SkipInjectTokenHeader);
      return next.handle(req.clone({headers}));
    } else {
      return this.authorize.getAccessToken()
        .pipe(mergeMap(token => this.processRequestWithToken(token, req, next)));
    }
  }

  private processRequestWithToken(token: string, req: HttpRequest<any>, next: HttpHandler) {
    if (!!token) {
      req = req.clone({
        setHeaders: {
          Authorization: `Bearer ${token}`
        }
      });
    }

    if (req.headers.has(SkipErrorRedirectHeader)) {
      const headers = req.headers.delete(SkipErrorRedirectHeader);
      return next.handle(req.clone({headers}));
    } else {
      return next.handle(req).pipe(catchError(err => {
        if (err.status === 401) {
          console.log('Unauthorized access, redirect user to login.');
          this.router.navigate(['/auth/login']);
        }
        return throwError(err);
      }));
    }
  }
}
