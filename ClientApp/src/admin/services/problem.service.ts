import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

import { ProblemEditDto, ProblemInfoDto } from '../../app/interfaces/problem.interfaces';
import { PaginatedList } from '../../app/interfaces/pagination.interfaces';

@Injectable({
  providedIn: 'root'
})
export class AdminProblemService {
  public cachedId: number;
  public cachedData: Observable<ProblemEditDto>;

  constructor(private http: HttpClient) {
  }

  public getPaginatedList(pageIndex: number): Observable<PaginatedList<ProblemInfoDto>> {
    return this.http.get<PaginatedList<ProblemInfoDto>>('/admin/problem', {
      params: new HttpParams().set('pageIndex', pageIndex.toString())
    });
  }

  public getSingle(problemId: number): Observable<ProblemEditDto> {
    return this.http.get<ProblemEditDto>('/admin/problem/' + problemId.toString());
  }

  public createSingle(problem: ProblemEditDto): Observable<any> {
    return this.http.post('/admin/problem', problem);
  }

  public updateSingle(problem: ProblemEditDto): Observable<ProblemEditDto> {
    return this.http.put<ProblemEditDto>('/admin/problem/' + problem.id.toString(), problem);
  }

  public deleteSingle(problemId: number): Observable<any> {
    return this.http.delete('/admin/problem/' + problemId.toString());
  }
}
