import {Injectable} from '@angular/core';
import {HttpClient, HttpParams} from '@angular/common/http';
import {Observable} from 'rxjs';
import {map} from 'rxjs/operators';

import {
  mapSubmissionInfoDtoFields, mapSubmissionViewDtoFields,
  mapSubmissionViewDtoListFields,
  SubmissionViewDto
} from '../../interfaces/submission.interfaces';

import {SubmissionReviewInfoDto} from "../../interfaces/submissionReview.interface";

@Injectable({
  providedIn: 'root'
})
export class ReviewFeedbackDService {

  constructor(
    private http: HttpClient,
  ) {
  }

  public getFeedbackList(problemId: number): Observable<SubmissionReviewInfoDto[]> {
    try {
      let params = new HttpParams();
      params = params.set("problemId", problemId.toString());
      return this.http.get<SubmissionReviewInfoDto[]>('/reviewFeedback', {params: params})
        .pipe(map(feedbacks => {
          for (let i = 0; i < feedbacks.length; ++i) {
            mapSubmissionViewDtoFields(feedbacks[i].submission);
          }
          return feedbacks;
        }));
    } catch (err) {
      throw err;
    }
  }
}
