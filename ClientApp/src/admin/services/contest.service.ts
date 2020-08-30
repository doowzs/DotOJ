import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

import { PaginatedList } from '../../app/interfaces/pagination.interfaces';
import { ContestCreateDto, ContestInfoDto } from '../../app/interfaces/contest.interfaces';

@Injectable({
  providedIn: 'root'
})
export class AdminContestService {
  constructor(private http: HttpClient) {
  }

  public getPaginatedList(pageIndex: number): Observable<PaginatedList<ContestInfoDto>> {
    return this.http.get<PaginatedList<ContestInfoDto>>('/admin/contest', {
      params: new HttpParams().set('pageIndex', pageIndex.toString())
    });
  }

  public createSingle(contest: ContestCreateDto): Observable<any> {
    return this.http.post('/admin/contest', contest);
  }
}
