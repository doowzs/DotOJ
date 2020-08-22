import {Inject, Injectable} from '@angular/core';
import {HttpClient, HttpParams} from '@angular/common/http';
import {Observable} from 'rxjs';

import {AssignmentEditDto, AssignmentListPagination, SubmissionViewDto} from 'src/interfaces';

@Injectable({
  providedIn: 'root'
})
export class AdminAssignmentService {
  private id: number;
  private cached: SubmissionViewDto;

  constructor(
    private http: HttpClient,
    @Inject('BASE_URL') private baseUrl: string
  ) {
  }

  public getPaginatedList(pageIndex: number): Observable<AssignmentListPagination> {
    return this.http.get<AssignmentListPagination>(this.baseUrl + 'api/v1/admin/assignment', {
      params: new HttpParams().set('pageIndex', pageIndex.toString())
    });
  }

  public getSingle(id: number): Observable<AssignmentEditDto> {
    return this.http.get<AssignmentEditDto>(this.baseUrl + 'api/v1/admin/assignment/' + id.toString());
  }

  public CreateSingle(assignment: AssignmentEditDto): Observable<AssignmentEditDto> {
    return this.http.post<AssignmentEditDto>(this.baseUrl + 'api/v1/admin/assignment', assignment);
  }
}
