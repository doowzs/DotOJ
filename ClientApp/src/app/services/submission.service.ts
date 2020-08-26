import { Injectable } from '@angular/core';
import { Observable, Subject } from 'rxjs';
import { HttpClient, HttpParams } from '@angular/common/http';
import { map, tap } from 'rxjs/operators';
import * as moment from 'moment';

import { SubmissionInfoDto } from '../interfaces/submission.interfaces';
import { AuthorizeService } from '../../api-authorization/authorize.service';
import { PaginatedList } from '../interfaces/pagination.interfaces';
import { Verdicts } from '../consts/verdicts.consts';

@Injectable({
  providedIn: 'root'
})
export class SubmissionService {
  public userId: Observable<string>; // TODO: used for query
  public newSubmission = new Subject<SubmissionInfoDto>();

  constructor(
    private http: HttpClient,
    private auth: AuthorizeService,
  ) {
    this.userId = this.auth.getUser().pipe(map(u => u && u.sub));
  }

  // TODO: add type for verdict
  public getPaginatedList(contestId: number | null, problemId: number | null, userId: string | null, verdict: any | null)
    : Observable<PaginatedList<SubmissionInfoDto>> {
    const params = new HttpParams();
    if (contestId !== null) {
      params.set('contestId', contestId.toString());
    }
    if (problemId !== null) {
      params.set('problemId', problemId.toString());
    }
    if (userId !== null) {
      params.set('userId', userId);
    }
    if (verdict !== null) {
      params.set('verdict', verdict.toString());
    }
    return this.http.get<PaginatedList<SubmissionInfoDto>>('/submission', { params: params })
      .pipe(map(list => {
        for (let i = 0; i < list.items.length; ++i) {
          const submission = list.items[i];
          submission.verdict = Verdicts.find(v => v.code === submission.verdict);
          submission.createdAt = moment.utc(submission.createdAt).local();
          submission.judgedAt = moment.utc(submission.judgedAt).local();
        }
        return list;
      }));
  }

  public createSingle(problemId: number, language: number, code: string): Observable<SubmissionInfoDto> {
    return this.http.post<SubmissionInfoDto>('/submission', {
      problemId: problemId,
      program: {
        language: language,
        code: code
      }
    }).pipe(tap(data => this.newSubmission.next(data)));
  }
}
