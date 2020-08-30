import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import * as moment from 'moment';

import { PaginatedList } from '../../app/interfaces/pagination.interfaces';
import { ContestCreateDto, ContestInfoDto } from '../../app/interfaces/contest.interfaces';
import { map } from 'rxjs/operators';

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

  public createSingle(contest: ContestCreateDto): Observable<any> {
    return this.http.post('/admin/contest', contest);
  }
}
