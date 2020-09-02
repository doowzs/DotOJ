import { Injectable } from '@angular/core';
import { Observable, Subject } from 'rxjs';
import { HttpClient, HttpParams } from '@angular/common/http';
import { map, tap } from 'rxjs/operators';

import { SubmissionInfoDto, SubmissionViewDto } from '../interfaces/submission.interfaces';
import { AuthorizeService } from '../../api-authorization/authorize.service';
import { PaginatedList } from '../interfaces/pagination.interfaces';
import {
  mapSubmissionInfoDtoFields,
  mapSubmissionViewDtoFields,
} from '../consts/verdicts.consts';

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
          list.items[i] = mapSubmissionInfoDtoFields(list.items[i]);
        }
        return list;
      }));
  }

  public getSingleAsInfo(submissionId: number): Observable<SubmissionInfoDto> {
    return this.http.get<SubmissionInfoDto>('/submission/' + submissionId.toString())
      .pipe(map(mapSubmissionInfoDtoFields));
  }

  public getSingleAsView(submissionId: number): Observable<SubmissionViewDto> {
    return this.http.get<SubmissionViewDto>('/submission/' + submissionId.toString() + '/detail')
      .pipe(map(mapSubmissionViewDtoFields));
  }

  public createSingle(problemId: number, language: number, code: string): Observable<SubmissionInfoDto> {
    return this.http.post<SubmissionInfoDto>('/submission', {
      problemId: problemId,
      program: {
        language: language,
        code: btoa(code)
      }
    }).pipe(map(mapSubmissionInfoDtoFields), tap(data => this.newSubmission.next(data)));
  }
}
