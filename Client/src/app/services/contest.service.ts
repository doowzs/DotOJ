import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map, shareReplay } from 'rxjs/operators';
import * as moment from 'moment';

import { PaginatedList } from '../../interfaces/pagination.interfaces';
import { ContestInfoDto, ContestViewDto } from '../../interfaces/contest.interfaces';
import { RegistrationInfoDto } from '../../interfaces/registration.interfaces';

@Injectable({
  providedIn: 'root'
})
export class ContestService {
  private cachedId: number;
  private cachedData: Observable<ContestViewDto>;

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

  public getSingle(contestId: number, force: boolean = false): Observable<ContestViewDto> {
    if (!force && this.cachedId === contestId && this.cachedData) {
      return this.cachedData;
    } else {
      this.cachedId = contestId;
      this.cachedData = null;
      return this.cachedData = this.http.get<ContestViewDto>('/contest/' + contestId.toString())
        .pipe(map(data => {
          for (let i = 0; i < data.problems.length; ++i) {
            data.problems[i].label = String.fromCharCode('A'.charCodeAt(0) + i);
          }
          data.beginTime = moment.utc(data.beginTime).local();
          data.endTime = moment.utc(data.endTime).local();
          if (!!data.scoreBonusTime) {
            data.scoreBonusTime = moment.utc(data.scoreBonusTime).local();
          }
          if (!!data.scoreDecayTime) {
            data.scoreDecayTime = moment.utc(data.scoreDecayTime).local();
          }
          return data;
        }), shareReplay(1));
    }
  }

  public getRegistrations(contestId: number): Observable<RegistrationInfoDto[]> {
    return this.http.get<RegistrationInfoDto[]>('/contest/' + contestId.toString() + '/registrations')
      .pipe(map(data => {
        for (let i = 0; i < data.length; ++i) {
          const registration = data[i];
          for (let j = 0; j < registration.statistics.length; ++j) {
            const statistic = registration.statistics[j];
            if (statistic.acceptedAt) {
              // Do not map acceptedAt to moment if value is null.
              statistic.acceptedAt = moment.utc(statistic.acceptedAt).local();
            }
          }
        }
        return data;
      }));
  }
}
