import {Inject, Injectable} from '@angular/core';
import {HttpClient, HttpParams} from '@angular/common/http';
import {Observable} from 'rxjs';
import {map, mergeMap} from 'rxjs/operators';

import {
  SubmissionInfoDto,
  SubmissionViewDto
} from '../app.interfaces';
import {AuthorizeService} from '../../api-authorization/authorize.service';

@Injectable({
  providedIn: 'root'
})
export class SubmissionService {
  private id: number;
  private cached: SubmissionViewDto;
  private userId: Observable<string>;

  constructor(
    private http: HttpClient,
    private auth: AuthorizeService,
    @Inject('BASE_URL') private baseUrl: string
  ) {
    this.userId = this.auth.getUser().pipe(map(u => u && u.sub));
  }

  public getListByProblem(problemId: number): Observable<SubmissionInfoDto[] | null> {
    return this.userId.pipe(mergeMap(userId => {
      return this.http.get<SubmissionInfoDto[]>(this.baseUrl + 'api/v1/submission', {
        params: new HttpParams().set('problemId', problemId.toString()).set('userId', userId)
      });
    }));
  }
}
