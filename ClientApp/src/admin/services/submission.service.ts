import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { SubmissionEditDto, SubmissionInfoDto } from '../../app/interfaces/submission.interfaces';
import { PaginatedList } from '../../app/interfaces/pagination.interfaces';
import { map } from 'rxjs/operators';
import { mapSubmissionEditDtoFields, mapSubmissionInfoDtoFields } from '../../app/consts/verdicts.consts';

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
    submission.message = btoa(submission.message);
    return this.http.put<SubmissionEditDto>('/admin/submission/' + submission.id.toString(), submission)
      .pipe(map(mapSubmissionEditDtoFields));
  }

  public deleteSingle(submissionId: number): Observable<any> {
    return this.http.delete('/admin/submission/' + submissionId.toString());
  }

  public rejudge(contestId: number | null, problemId: number | null, submissionId: number | null): Observable<SubmissionInfoDto[]> {
    return this.http.post<SubmissionInfoDto[]>('/admin/submission/rejudge', {
      contestId: contestId,
      problemId: problemId,
      submissionId: submissionId
    }).pipe(map(list => {
      for (let i = 0; i < list.length; ++i) {
        const submission = list[i];
        list[i] = mapSubmissionInfoDtoFields(submission);
      }
      return list;
    }));
  }
}
