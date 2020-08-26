import { Injectable } from '@angular/core';
import { Observable, Subject } from 'rxjs';
import { SubmissionInfoDto } from '../interfaces/submission.interfaces';
import { AuthorizeService } from '../../api-authorization/authorize.service';
import { HttpClient } from '@angular/common/http';
import { map, tap } from 'rxjs/operators';

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
