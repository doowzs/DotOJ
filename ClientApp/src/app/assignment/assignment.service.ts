import {Inject, Injectable} from '@angular/core';
import {HttpClient, HttpParams} from '@angular/common/http';
import {Observable, of} from 'rxjs';
import {map} from 'rxjs/operators';

import {
  AssignmentListPagination,
  AssignmentViewDto
} from 'src/interfaces';

@Injectable({
  providedIn: 'root'
})
export class AssignmentService {
  private id: number;
  private cached: AssignmentViewDto;

  constructor(
    private http: HttpClient,
    @Inject('BASE_URL') private baseUrl: string
  ) {
  }

  public getPaginatedList(pageIndex: number): Observable<AssignmentListPagination> {
    return this.http.get<AssignmentListPagination>(this.baseUrl + 'api/v1/assignment', {
      params: new HttpParams().set('pageIndex', pageIndex.toString())
    });
  }

  public getSingle(id: number): Observable<AssignmentViewDto | null> {
    if (this.id === id && this.cached) {
      return of(this.cached);
    } else {
      this.id = id;
      this.cached = null;
      return this.http.get<AssignmentViewDto>(this.baseUrl + `api/v1/assignment/${id}`)
        .pipe(map(data => {
          for (let i = 0; i < data.problems.length; ++i) {
            data.problems[i].label = String.fromCharCode('A'.charCodeAt(0) + i);
          }
          return this.cached = data;
        }));
    }
  }
}
