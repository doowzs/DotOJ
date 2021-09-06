import { Inject, Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable()
export class ApplicationApiInterceptor implements HttpInterceptor {
  constructor(@Inject('BASE_URL') private baseUrl: string) {
  }

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    if (req.url.startsWith('[root]')) {
      req = req.clone({ url: this.baseUrl + req.url.substr(6) });
    } else {
      req = req.clone({ url: this.baseUrl + 'api/v2' + req.url });
    }
    return next.handle(req);
  }
}
