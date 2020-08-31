import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import * as moment from 'moment';

import { PaginatedList } from '../../app/interfaces/pagination.interfaces';
import { ContestEditDto, ContestInfoDto } from '../../app/interfaces/contest.interfaces';

@Injectable({
  providedIn: 'root'
})
export class AdminContestService {
  constructor(private http: HttpClient) {
  }

  public getPaginatedList(pageIndex: number): Observable<PaginatedList<ContestInfoDto>> {
    return this.http.get<PaginatedList<ContestInfoDto>>('/admin/contest', {
      params: new HttpParams().set('pageIndex', pageIndex.toString())
    }).pipe(map(list => {
      for (let i = 0; i < list.items.length; ++i) {
        const contest = list.items[i];
        contest.beginTime = moment.utc(contest.beginTime).local();
        contest.endTime = moment.utc(contest.endTime).local();
      }
      return list;
    }));
  }

  public getSingle(contestId: number): Observable<ContestEditDto> {
    return this.http.get<ContestEditDto>('/admin/contest/' + contestId.toString());
  }

  public createSingle(contest: ContestEditDto): Observable<any> {
    return this.http.post('/admin/contest', contest);
  }

  public updateSingle(contest: ContestEditDto): Observable<ContestEditDto> {
    return this.http.put<ContestEditDto>('/admin/contest/' + contest.id.toString(), contest);
  }

  public deleteSingle(contestId: number): Observable<any> {
    return this.http.delete('/admin/contest/' + contestId.toString());
  }
}

