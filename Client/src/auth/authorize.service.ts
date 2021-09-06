import {Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {Observable, of} from 'rxjs';
import {map, tap} from 'rxjs/operators';
import * as moment from "moment";

const key = 'dotoj-auth';

export interface IUser {
  id: string;
  username: string;
  fullName: string;
  roles: string[];
  token: string;
  issued: string;
  expires: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthorizeService {

  constructor(private http: HttpClient) {
  }

  public authenticate(username: string, password: string): Observable<IUser> {
    console.log(username, password);
    return this.http.post<IUser>('/auth/login', {
      username: username,
      password: password
    }).pipe(tap(u => window.localStorage.setItem(key, JSON.stringify(u))));
  }

  public removeCredentials() : void {
    window.localStorage.removeItem(key);
  }

  public getUser(): Observable<IUser | null> {
    const item = localStorage.getItem(key);
    return of(!key ? null : JSON.parse(item))
      .pipe(tap(user => {
        if (user != null && moment.utc().isAfter(moment.utc(user.issued).add(1, "minutes"))) {
          this.http.post<IUser>('/auth/refresh', {})
            .subscribe(u => window.localStorage.setItem(key, JSON.stringify(u)));
        }
      }));
  }

  public isAuthenticated(): Observable<boolean> {
    return this.getUser().pipe(map(u => !!u && moment.utc().isBefore(moment.utc(u.expires))));
  }

  public getAccessToken(): Observable<string> {
    return this.getUser().pipe(map(u => !!u && u.token));
  }

}
