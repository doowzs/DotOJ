import { Inject, Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable()
export class ApplicationApiInterceptor implements HttpInterceptor {
  constructor(@Inject('BASE_URL') private baseUrl: string) {
  }

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const url = this.baseUrl + 'api/v1';
    req = req.clone({ url: url + req.url });
    return next.handle(req);
  }
}
