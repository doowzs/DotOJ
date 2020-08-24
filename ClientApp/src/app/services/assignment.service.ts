import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { PaginatedList } from '../interfaces/pagination.interfaces';
import { AssignmentInfoDto } from '../interfaces/assignment.interfaces';

@Injectable({
  providedIn: 'root'
})
export class AssignmentService {
  constructor(private http: HttpClient) {
  }

  public getOngoingList(): Observable<AssignmentInfoDto[]> {
    return this.http.get<AssignmentInfoDto[]>('/assignment/ongoing');
  }

  public getPaginatedList(page?: number): Observable<PaginatedList<AssignmentInfoDto>> {
    return this.http.get<PaginatedList<AssignmentInfoDto>>('/assignment');
}
}
