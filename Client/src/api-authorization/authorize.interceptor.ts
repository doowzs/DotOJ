import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { AuthorizeService } from './authorize.service';
import { catchError, mergeMap } from 'rxjs/operators';
import { Router } from '@angular/router';

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
    return this.authorize.getAccessToken()
      .pipe(mergeMap(token => this.processRequestWithToken(token, req, next)));
  }

  // Checks if there is an access_token available in the authorize service
  // and adds it to the request in case it's targeted at the same origin as the
  // single page application.
  private processRequestWithToken(token: string, req: HttpRequest<any>, next: HttpHandler) {
    if (!!token && this.isSameOriginUrl(req)) {
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
          this.router.navigate(['/authentication/login']);
        }
        return throwError(err);
      }));
    }
  }

  private isSameOriginUrl(req: any) {
    // It's an absolute url with the same origin.
    if (req.url.startsWith(`${window.location.origin}/`)) {
      return true;
    }

    // It's a protocol relative url with the same origin.
    // For example: //www.example.com/api/Products
    if (req.url.startsWith(`//${window.location.host}/`)) {
      return true;
    }

    // It's a relative url like /api/Products
    if (/^\/[^\/].*/.test(req.url)) {
      return true;
    }

    // It's an absolute or protocol relative url that
    // doesn't have the same origin.
    return false;
  }
}
