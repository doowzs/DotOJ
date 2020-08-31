import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PaginatedList } from '../../app/interfaces/pagination.interfaces';
import { UserEditDto, UserInfoDto } from '../../app/interfaces/user.interfaces';

@Injectable({
  providedIn: 'root'
})
export class AdminUserService {
  constructor(private http: HttpClient) {
  }

  public getPaginatedList(pageIndex: number): Observable<PaginatedList<UserInfoDto>> {
    return this.http.get<PaginatedList<UserInfoDto>>('/admin/user', {
      params: new HttpParams().append('pageIndex', pageIndex.toString());
    });
  }

  public getSingle(id: string): Observable<UserEditDto> {
    return this.http.get<UserEditDto>('/admin/user/' + id);
  }
}
