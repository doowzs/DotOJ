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
  private cachedData: ProblemViewDto;

  constructor(private http: HttpClient) {
  }

  public getSingle(problemId: number): Observable<ProblemViewDto> {
    if (this.cachedId === problemId && this.cachedData) {
      return of(this.cachedData);
    } else {
      this.cachedId = problemId;
      this.cachedData = null;
      return this.http.get<ProblemViewDto>('/problem/' + problemId.toString())
        .pipe(map(data => {
          // TODO: fix timestamps
          return data;
        }), tap(data => this.cachedData = data));
    }
  }
}
