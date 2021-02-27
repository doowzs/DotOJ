import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, Subject } from 'rxjs';
import { map, take, tap } from 'rxjs/operators';

import { Program, SubmissionInfoDto, SubmissionViewDto } from '../../interfaces/submission.interfaces';
import { AuthorizeService } from '../../api-authorization/authorize.service';
import { PaginatedList } from '../../interfaces/pagination.interfaces';
import { mapSubmissionInfoDtoFields, mapSubmissionViewDtoFields } from '../../interfaces/submission.interfaces';

@Injectable({
  providedIn: 'root'
})
export class SubmissionService {
  public userId: Observable<string>;
  public newSubmission = new Subject<SubmissionInfoDto>();

  constructor(
    private http: HttpClient,
    private auth: AuthorizeService,
  ) {
    this.userId = this.auth.getUser().pipe(take(1), map(u => u && u.sub));
  }

  public getPaginatedList(contestId: number | null, userId: string | null, contestantId: string | null, problemId: number | null,
                          verdict: number | null, pageSize: number | null, pageIndex: number | null): Observable<PaginatedList<SubmissionInfoDto>> {
    let params = new HttpParams();
    if (contestId) {
      params = params.set('contestId', contestId.toString());
    }
    if (userId) {
      params = params.set('userId', userId);
    }
    if (contestantId) {
      params = params.set('contestantId', contestantId);
    }
    if (problemId) {
      params = params.set('problemId', problemId.toString());
    }
    if (verdict) {
      params = params.set('verdict', verdict.toString());
    }
    if (pageSize) {
      params = params.set('pageSize', pageSize.toString());
    }
    if (pageIndex) {
      params = params.set('pageIndex', pageIndex.toString());
    }

    return this.http.get<PaginatedList<SubmissionInfoDto>>('/submission', {params: params})
      .pipe(map(list => {
        for (let i = 0; i < list.items.length; ++i) {
          list.items[i] = mapSubmissionInfoDtoFields(list.items[i]);
        }
        return list;
      }));
  }

  public getBatchInfos(submissionIds: number[]): Observable<SubmissionInfoDto[]> {
    if (submissionIds.length === 0) {
      return;
    }

    let params = new HttpParams();
    for (let i = 0; i < submissionIds.length; ++i) {
      params = params.append('id', submissionIds[i].toString());
    }
    return this.http.get<SubmissionInfoDto[]>('/submission/batch', {params: params})
      .pipe(map(list => {
        for (let i = 0; i < list.length; ++i) {
          list[i] = mapSubmissionInfoDtoFields(list[i]);
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

  public createSingle(problemId: number, program: Program): Observable<SubmissionInfoDto> {
    return this.http.post<SubmissionInfoDto>('/submission', {
      problemId: problemId,
      program: program
    }).pipe(map(mapSubmissionInfoDtoFields), tap(data => this.newSubmission.next(data)));
  }

  public getSubmitToken(problemId: number): Observable<string> {
    return this.http.get<string>('/submission/testkit/token', {
      params: {
        problemId: problemId.toString()
      }
    });
  }
}
