import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { PaginatedList } from '../interfaces/pagination.interfaces';
import { ContestInfoDto } from '../interfaces/contest.interfaces';

@Injectable({
  providedIn: 'root'
})
export class ContestService {
  constructor(private http: HttpClient) {
  }

  public getOngoingList(): Observable<ContestInfoDto[]> {
    return this.http.get<ContestInfoDto[]>('/assignment/ongoing');
  }

  public getPaginatedList(page?: number): Observable<PaginatedList<ContestInfoDto>> {
    return this.http.get<PaginatedList<ContestInfoDto>>('/assignment');
}
}
