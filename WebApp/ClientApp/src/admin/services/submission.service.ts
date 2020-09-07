import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { Base64 } from 'js-base64';

import { SubmissionEditDto, SubmissionInfoDto } from '../../app/interfaces/submission.interfaces';
import { PaginatedList } from '../../app/interfaces/pagination.interfaces';
import { mapSubmissionEditDtoFields, mapSubmissionInfoDtoFields } from '../../app/interfaces/submission.interfaces';

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
