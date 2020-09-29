import { Injectable } from '@angular/core';
import { HttpClient, } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { Base64 } from 'js-base64';

import { ProblemViewDto } from '../../interfaces/problem.interfaces';

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
          for (let i = 0; i < data.sampleCases.length; ++i) {
            const sampleCase = data.sampleCases[i];
            sampleCase.input = Base64.decode(sampleCase.input);
            sampleCase.output = Base64.decode(sampleCase.output);
          }
          return data;
        }));
    }
  }
}
