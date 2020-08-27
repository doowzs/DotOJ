import { Injectable } from '@angular/core';
import { HttpClient, } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { map, tap } from 'rxjs/operators';

import { ProblemViewDto } from '../interfaces/problem.interfaces';

@Injectable({
  providedIn: 'root'
})
export class ProblemService {
  private cachedId: number;
  private cachedData: Observable<ProblemViewDto>;

  constructor(private http: HttpClient) {
  }

  public getSingle(problemId: number): Observable<ProblemViewDto> {
    if (this.cachedId === problemId && this.cachedData) {
      return this.cachedData;
    } else {
      this.cachedId = problemId;
      this.cachedData = null;
      return this.cachedData = this.http.get<ProblemViewDto>('/problem/' + problemId.toString())
        .pipe(map(data => {
          return data;
        }));
    }
  }
}
