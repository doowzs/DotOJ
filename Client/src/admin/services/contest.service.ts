import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import * as moment from 'moment';

import { PaginatedList } from '../../interfaces/pagination.interfaces';
import { ContestEditDto, ContestInfoDto } from '../../interfaces/contest.interfaces';
import { RegistrationInfoDto } from '../../interfaces/registration.interfaces';

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
    return this.http.get<ContestEditDto>('/admin/contest/' + contestId.toString())
      .pipe(map(data => {
        data.beginTime = moment.utc(data.beginTime).local();
        data.endTime = moment.utc(data.endTime).local();
        if (!!data.scoreBonusTime) {
          data.scoreBonusTime = moment.utc(data.scoreBonusTime).local();
        }
        if (!!data.scoreDecayTime) {
          data.scoreDecayTime = moment.utc(data.scoreDecayTime).local();
        }
        return data;
      }));
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

  public getRegistrations(contestId: number): Observable<RegistrationInfoDto[]> {
    return this.http.get<RegistrationInfoDto[]>('/admin/contest/' + contestId.toString() + '/registrations');
  }

  public addRegistrations(contestId: number, userIds: string[], isParticipant: boolean,
                          isContestManager: boolean): Observable<RegistrationInfoDto[]> {
    return this.http.post<RegistrationInfoDto[]>('/admin/contest/' + contestId.toString() + '/registrations', userIds, {
      params: new HttpParams()
        .append('isParticipant', isParticipant.toString())
        .append('isContestManager', isContestManager.toString())
    });
  }

  public removeRegistrations(contestId: number, userIds: string[]): Observable<any> {
    return this.http.delete('/admin/contest/' + contestId.toString() + '/registrations', {
      headers: new HttpHeaders().append('Content-Type', 'application/json'),
      params: new HttpParams().append('userIds', userIds.toString())
    });
  }

  public copyRegistrations(contestId: number, fromId: number): Observable<RegistrationInfoDto[]> {
    return this.http.post<RegistrationInfoDto[]>('/admin/contest/' + contestId.toString() + '/registrations/copy', {}, {
      params: new HttpParams().append('contestId', fromId.toString())
    });
  }
}

