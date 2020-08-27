import { Injectable } from '@angular/core';
import { Observable, Subject } from 'rxjs';
import { HttpClient, HttpParams } from '@angular/common/http';
import { map, tap } from 'rxjs/operators';
import * as moment from 'moment';

import { SubmissionInfoDto } from '../interfaces/submission.interfaces';
import { AuthorizeService } from '../../api-authorization/authorize.service';
import { PaginatedList } from '../interfaces/pagination.interfaces';
import { VerdictInfo, Verdicts } from '../consts/verdicts.consts';
import { Languages } from '../consts/languages.consts';

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

  public getPaginatedList(contestId: number | null, problemId: number | null, userId: string | null, verdict: VerdictInfo | null)
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
      params.set('verdict', verdict.code.toString());
    }
    return this.http.get<PaginatedList<SubmissionInfoDto>>('/submission', { params: params })
      .pipe(map(list => {
        for (let i = 0; i < list.items.length; ++i) {
          const submission = list.items[i];
          submission.verdict = Verdicts.find(v => v.code === submission.verdict);
          submission.language = Languages.find(l => l.code === submission.language);
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
    }).pipe(map(data => {
      data.verdict = Verdicts.find(v => v.code === data.verdict);
      data.language = Languages.find(l => l.code === data.language);
      data.createdAt = moment.utc(data.createdAt).local();
      data.judgedAt = moment.utc(data.judgedAt).local();
      return data;
    }), tap(data => this.newSubmission.next(data)));
  }
}
