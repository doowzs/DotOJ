import { Injectable } from '@angular/core';
import { HttpClient, HttpEvent, HttpParams, HttpRequest } from '@angular/common/http';
import { Observable } from 'rxjs';

import { ProblemEditDto, ProblemInfoDto, TestCase } from '../../app/interfaces/problem.interfaces';
import { PaginatedList } from '../../app/interfaces/pagination.interfaces';

@Injectable({
  providedIn: 'root'
})
export class AdminProblemService {
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

  public getTestCases(problemId: number): Observable<TestCase[]> {
    return this.http.get<TestCase[]>('/admin/problem/' + problemId.toString() + '/test-cases');
  }

  public uploadTestCases(problemId: number, file: File): Observable<HttpEvent<any>> {
    const formData = new FormData();
    formData.append('zip-file', file, file.name);

    const endpoint = '/admin/problem/' + problemId.toString() + '/test-cases';
    const options = {
      params: new HttpParams(),
      reportProgress: true
    };

    const request = new HttpRequest('POST', endpoint, formData, options);
    return this.http.request(request);
  }
}
