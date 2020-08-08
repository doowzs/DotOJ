import {Inject, Injectable} from '@angular/core';
import {HttpClient, HttpParams} from '@angular/common/http';
import {Observable, Subject} from 'rxjs';
import {map, mergeMap, take, tap} from 'rxjs/operators';

import {
  ProblemViewDto,
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
  public newSubmission = new Subject<SubmissionInfoDto>();

  constructor(
    private http: HttpClient,
    private auth: AuthorizeService,
    @Inject('BASE_URL') private baseUrl: string
  ) {
    this.userId = this.auth.getUser().pipe(map(u => u && u.sub));
  }

  public isJudging(submission: SubmissionInfoDto) {
    return submission.verdict >= 0 && submission.verdict <= 2;
  }

  public getListByProblem(problem: ProblemViewDto): Observable<SubmissionInfoDto[]> {
    return this.userId.pipe(take(1), mergeMap(userId => {
      return this.http.get<SubmissionInfoDto[]>(this.baseUrl + 'api/v1/submission/problem-user', {
        params: new HttpParams().set('problemId', problem.id.toString()).set('userId', userId)
      });
    }));
  }

  public getSingle(id: number, simple: boolean): Observable<SubmissionViewDto> {
    return this.http.get<SubmissionViewDto>(this.baseUrl + 'api/v1/submission/' + id.toString(), {
      params: new HttpParams().set('simple', simple.toString())
    });
  }

  public createSingle(problem: ProblemViewDto, language: number, code: string): Observable<SubmissionInfoDto> {
    return this.userId.pipe(take(1), mergeMap(userId => {
      return this.http.post<SubmissionInfoDto>(this.baseUrl + 'api/v1/submission', {
        problemId: problem.id,
        userId: userId,
        program: {
          language: language,
          code: code
        }
      }).pipe(tap(data => this.newSubmission.next(data)));
    }));
  }
}
