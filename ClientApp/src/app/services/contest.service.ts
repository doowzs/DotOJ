import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import * as moment from 'moment';

import { PaginatedList } from '../interfaces/pagination.interfaces';
import { ContestInfoDto } from '../interfaces/contest.interfaces';

@Injectable({
  providedIn: 'root'
})
export class ContestService {
  constructor(private http: HttpClient) {
  }

  public getUpcomingList(): Observable<ContestInfoDto[]> {
    return this.http.get<ContestInfoDto[]>('/contest/upcoming')
      .pipe(map((list: ContestInfoDto[]) => {
        for (let i = 0; i < list.length; ++i) {
          list[i].beginTime = moment(list[i].beginTime);
          list[i].endTime = moment(list[i].endTime);
        }
        return list;
      }));
  }

  public getPaginatedList(page?: number): Observable<PaginatedList<ContestInfoDto>> {
    return this.http.get<PaginatedList<ContestInfoDto>>('/assignment');
}
}
