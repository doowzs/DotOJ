import { Injectable } from '@angular/core';
import { Observable, Subject } from 'rxjs';
import { HttpClient, HttpParams } from '@angular/common/http';
import { map, tap } from 'rxjs/operators';
import * as moment from 'moment';

import { SubmissionInfoDto, SubmissionViewDto } from '../interfaces/submission.interfaces';
import { AuthorizeService } from '../../api-authorization/authorize.service';
import { PaginatedList } from '../interfaces/pagination.interfaces';
import { fixSubmissionREVerdictCode, Verdicts } from '../consts/verdicts.consts';
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

  private mapInfoFields(data: SubmissionInfoDto): SubmissionInfoDto {
    fixSubmissionREVerdictCode(data);
    data.verdict = Verdicts.find(v => v.code === data.verdict);
    data.language = Languages.find(l => l.code === data.language);
    data.createdAt = moment.utc(data.createdAt).local();
    data.judgedAt = moment.utc(data.judgedAt).local();
    return data;
  }

  private mapViewFields(data: SubmissionViewDto): SubmissionViewDto {
    fixSubmissionREVerdictCode(data);
    data.verdict = Verdicts.find(v => v.code === data.verdict);
    data.program.language = Languages.find(l => l.code === data.program.language);
    data.message = atob(data.message);
    data.createdAt = moment.utc(data.createdAt).local();
    data.judgedAt = moment.utc(data.judgedAt).local();
    return data;
  }

  public getPaginatedList(contestId: number | null, problemId: number | null, userId: string | null, verdict: number | null,
                          pageIndex: number | null): Observable<PaginatedList<SubmissionInfoDto>> {
    let params = new HttpParams();
    if (contestId) {
      params = params.set('contestId', contestId.toString());
    }
    if (problemId) {
      params = params.set('problemId', problemId.toString());
    }
    if (userId) {
      params = params.set('userId', userId);
    }
    if (verdict) {
      params = params.set('verdict', verdict.toString());
    }
    if (pageIndex) {
      params = params.set('pageIndex', pageIndex.toString());
    }

    return this.http.get<PaginatedList<SubmissionInfoDto>>('/submission', { params: params })
      .pipe(map(list => {
        for (let i = 0; i < list.items.length; ++i) {
          list.items[i] = this.mapInfoFields(list.items[i]);
        }
        return list;
      }));
  }

  public getSingleAsInfo(submissionId: number): Observable<SubmissionInfoDto> {
    return this.http.get<SubmissionInfoDto>('/submission/' + submissionId.toString())
      .pipe(map(this.mapInfoFields));
  }

  public getSingleAsView(submissionId: number): Observable<SubmissionViewDto> {
    return this.http.get<SubmissionViewDto>('/submission/' + submissionId.toString() + '/detail')
      .pipe(map(this.mapViewFields));
  }

  public createSingle(problemId: number, language: number, code: string): Observable<SubmissionInfoDto> {
    return this.http.post<SubmissionInfoDto>('/submission', {
      problemId: problemId,
      program: {
        language: language,
        code: btoa(code)
      }
    }).pipe(map(this.mapInfoFields), tap(data => this.newSubmission.next(data)));
  }
}
