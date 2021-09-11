import {Injectable} from '@angular/core';
import {HttpClient, HttpEvent, HttpParams, HttpRequest} from '@angular/common/http';
import {Observable, Subject} from 'rxjs';
import {map, take, tap} from 'rxjs/operators';

import {Program, SubmissionInfoDto, SubmissionViewDto} from '../../interfaces/submission.interfaces';
import {AuthorizeService} from '../../auth/authorize.service';
import {PaginatedList} from '../../interfaces/pagination.interfaces';
import {mapSubmissionInfoDtoFields, mapSubmissionViewDtoFields} from '../../interfaces/submission.interfaces';

@Injectable({
  providedIn: 'root'
})
export class SubmissionReviewService {

  constructor(
    private http: HttpClient,
  ) {
  }

  public getReviewList(problemId: number): Observable<SubmissionViewDto[]> {
    try {
      let params = new HttpParams();
      params = params.set("problemId", problemId.toString());
      return this.http.get<SubmissionViewDto[]>('/submissionReview', {params: params});
    } catch (err) {
      throw err;
    }
  }
}
