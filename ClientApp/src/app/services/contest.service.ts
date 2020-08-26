import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import * as moment from 'moment';

import { PaginatedList } from '../interfaces/pagination.interfaces';
import { ContestInfoDto, ContestViewDto } from '../interfaces/contest.interfaces';

@Injectable({
  providedIn: 'root'
})
export class ContestService {
  private cachedId: number;
  private cachedData: ContestViewDto;

  constructor(private http: HttpClient) {
  }

  public getCurrentList(): Observable<ContestInfoDto[]> {
    return this.http.get<ContestInfoDto[]>('/contest/current')
      .pipe(map((list: ContestInfoDto[]) => {
        for (let i = 0; i < list.length; ++i) {
          const contest = list[i];
          contest.beginTime = moment.utc(contest.beginTime).local();
          contest.endTime = moment.utc(contest.endTime).local();
        }
        return list;
      }));
  }

  public getPaginatedList(pageIndex: number): Observable<PaginatedList<ContestInfoDto>> {
    return this.http.get<PaginatedList<ContestInfoDto>>('/contest', {
      params: new HttpParams().set('pageIndex', pageIndex.toString())
    }).pipe(map((list: PaginatedList<ContestInfoDto>) => {
      for (let i = 0; i < list.items.length; ++i) {
        const contest = list.items[i];
        contest.beginTime = moment.utc(contest.beginTime).local();
        contest.endTime = moment.utc(contest.endTime).local();
      }
      return list;
    }));
  }

  public getSingle(contestId: number): Observable<ContestViewDto> {
    if (this.cachedId === contestId && this.cachedData) {
      return of(this.cachedData);
    } else {
      this.cachedId = contestId;
      this.cachedData = null;
      return this.http.get<ContestViewDto>('/contest/' + contestId.toString())
        .pipe(map(data => {
          for (let i = 0; i < data.problems.length; ++i) {
            data.problems[i].label = String.fromCharCode('A'.charCodeAt(0) + i);
          }
          return data;
        }), tap(data => this.cachedData = data));
    }
  }
}
