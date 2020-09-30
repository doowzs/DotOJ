import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PaginatedList } from '../../interfaces/pagination.interfaces';
import { UserEditDto, UserInfoDto } from '../../interfaces/user.interfaces';

@Injectable({
  providedIn: 'root'
})
export class AdminUserService {
  constructor(private http: HttpClient) {
  }

  public getPaginatedList(pageIndex: number): Observable<PaginatedList<UserInfoDto>> {
    return this.http.get<PaginatedList<UserInfoDto>>('/admin/user', {
      params: new HttpParams().append('pageIndex', pageIndex.toString())
    });
  }

  public getSingle(id: string): Observable<UserEditDto> {
    return this.http.get<UserEditDto>('/admin/user/' + id);
  }

  public updateSingle(id: string, user: UserEditDto): Observable<UserEditDto> {
    return this.http.put<UserEditDto>('/admin/user/' + id, user);
  }

  public deleteSingle(id: string): Observable<any> {
    return this.http.delete('/admin/user/' + id);
  }
}
