import {Inject, Injectable} from '@angular/core';
import {HttpClient, HttpParams} from '@angular/common/http';
import {Observable, of, Subject} from 'rxjs';
import {map, mergeMap, take, tap} from 'rxjs/operators';

import {
  ProblemViewDto,
  SubmissionInfoDto,
  SubmissionViewDto
} from 'src/interfaces';
import {Verdicts} from 'src/consts';
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
    return Verdicts.find(v => v.code === submission.verdict)?.stage === 0;
  }

  public getListByProblem(problem: ProblemViewDto): Observable<SubmissionInfoDto[]> {
    return this.userId.pipe(take(1), mergeMap(userId => {
      return this.http.get<SubmissionInfoDto[]>(this.baseUrl + 'api/v1/submission/problem-user', {
        params: new HttpParams().set('problemId', problem.id.toString()).set('userId', userId)
      });
    }));
  }

  public getSingle(id: number, simple: boolean = false): Observable<SubmissionViewDto> {
    if (this.id === id && this.cached) {
      return of(this.cached);
    } else {
      this.id = id;
      this.cached = null;
      return this.http.get<SubmissionViewDto>(this.baseUrl + 'api/v1/submission/' + id.toString(), {
        params: new HttpParams().set('simple', simple.toString())
      }).pipe(tap(data => {
        if (!simple) {
          this.cached = data;
        }
      }));
    }
  }

  public getSingleAsInfo(id: number): Observable<SubmissionInfoDto> {
    return this.getSingle(id, true).pipe(map(data => data as SubmissionInfoDto));
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
