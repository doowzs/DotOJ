import {Inject, Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {Observable} from 'rxjs';

import {AssignmentEditDto, SubmissionViewDto} from 'src/interfaces';

@Injectable({
  providedIn: 'root'
})
export class AssignmentService {
  private id: number;
  private cached: SubmissionViewDto;

  constructor(
    private http: HttpClient,
    @Inject('BASE_URL') private baseUrl: string
  ) {
  }

  public CreateSingle(assignment: AssignmentEditDto): Observable<AssignmentEditDto> {
    return this.http.post<AssignmentEditDto>(this.baseUrl + 'api/v1/admin/assignment', assignment);
  }
}
