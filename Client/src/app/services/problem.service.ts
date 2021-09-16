import { Injectable } from '@angular/core';
import { HttpClient, HttpParams} from '@angular/common/http';
import { Observable } from 'rxjs';
import { map, shareReplay } from 'rxjs/operators';
import { Base64 } from 'js-base64';

import { ProblemViewDto } from '../../interfaces/problem.interfaces';
import {hackInfo} from "../../interfaces/hackScore.interfaces";

@Injectable({
  providedIn: 'root'
})
export class ProblemService {
  private cachedId: number;
  private cachedData: Observable<ProblemViewDto>;

  constructor(private http: HttpClient) {
  }

  public getSingle(problemId: number, force: boolean = false): Observable<ProblemViewDto> {
    if (!force && this.cachedId === problemId && this.cachedData) {
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
        }), shareReplay(1));
    }
  }

  public getHackInfo(problemId: number): Observable<hackInfo[]> {
    let params = new HttpParams();
    params = params.set("problemId", problemId.toString());
    return this.http.get<hackInfo[]>('/problem/' + problemId.toString() + '/HackDownload', {params: params});
  }
}
