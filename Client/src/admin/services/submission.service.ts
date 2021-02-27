import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { Base64 } from 'js-base64';

import { Program, SubmissionEditDto, SubmissionInfoDto } from '../../interfaces/submission.interfaces';
import { PaginatedList } from '../../interfaces/pagination.interfaces';
import { mapSubmissionEditDtoFields, mapSubmissionInfoDtoFields } from '../../interfaces/submission.interfaces';

@Injectable({
  providedIn: 'root'
})
export class AdminSubmissionService {
  constructor(private http: HttpClient) {
  }

  public getPaginatedList(pageIndex: number): Observable<PaginatedList<SubmissionInfoDto>> {
    return this.http.get<PaginatedList<SubmissionInfoDto>>('/admin/submission', {
      params: new HttpParams().append('pageIndex', pageIndex.toString())
    }).pipe(map(list => {
      for (let i = 0; i < list.items.length; ++i) {
        const submission = list.items[i];
        list.items[i] = mapSubmissionInfoDtoFields(submission);
      }
      return list;
    }));
  }

  public getSingle(submissionId: number): Observable<SubmissionEditDto> {
    return this.http.get<SubmissionEditDto>('/admin/submission/' + submissionId.toString())
      .pipe(map(mapSubmissionEditDtoFields));
  }

  public getBatchInfos(submissionIds: number[]): Observable<SubmissionInfoDto[]> {
    if (submissionIds.length === 0) {
      return;
    }

    let params = new HttpParams();
    for (let i = 0; i < submissionIds.length; ++i) {
      params = params.append('id', submissionIds[i].toString());
    }
    return this.http.get<SubmissionInfoDto[]>('/admin/submission/batch', { params: params })
      .pipe(map(list => {
        for (let i = 0; i < list.length; ++i) {
          list[i] = mapSubmissionInfoDtoFields(list[i]);
        }
        return list;
      }));
  }

  public createSingle(problemId: number, program: Program): Observable<SubmissionInfoDto> {
    return this.http.post<SubmissionInfoDto>('/admin/submission', {
      problemId: problemId,
      program: program
    }).pipe(map(mapSubmissionInfoDtoFields));
  }

  public updateSingle(submission: SubmissionEditDto): Observable<SubmissionEditDto> {
    submission.message = Base64.encode(submission.message ?? '');
    return this.http.put<SubmissionEditDto>('/admin/submission/' + submission.id.toString(), submission)
      .pipe(map(mapSubmissionEditDtoFields));
  }

  public deleteSingle(submissionId: number): Observable<any> {
    return this.http.delete('/admin/submission/' + submissionId.toString());
  }

  public rejudge(contestId: number | null, problemId: number | null, submissionId: number | null): Observable<SubmissionInfoDto[]> {
    if (contestId == null && problemId == null && submissionId == null) {
      return;
    }

    let params = new HttpParams();
    if (contestId) {
      params = params.set('contestId', contestId.toString());
    }
    if (problemId) {
      params = params.set('problemId', problemId.toString());
    }
    if (submissionId) {
      params = params.set('submissionId', submissionId.toString());
    }

    return this.http.post<SubmissionInfoDto[]>('/admin/submission/rejudge', {}, { params: params })
      .pipe(map(list => {
        for (let i = 0; i < list.length; ++i) {
          const submission = list[i];
          list[i] = mapSubmissionInfoDtoFields(submission);
        }
        return list;
      }));
  }
}
