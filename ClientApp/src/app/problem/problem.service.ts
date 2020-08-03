import {Inject, Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {Observable, of} from 'rxjs';
import {tap} from 'rxjs/operators';

import {ProblemViewDto} from '../app.interfaces';

@Injectable({
  providedIn: 'root'
})
export class ProblemService {
  private id: number;
  private cached: ProblemViewDto;

  constructor(
    private http: HttpClient,
    @Inject('BASR_URL') private baseUrl: string
  ) {
  }

  public getSingle(id: number): Observable<ProblemViewDto | null> {
    if (this.id === id && this.cached) {
      return of(this.cached);
    } else {
      this.id = id;
      this.cached = null;
      return this.http.get<ProblemViewDto>(this.baseUrl + `/api/v1/problem/${id}`)
        .pipe(tap(data => this.cached = data));
    }
  }
}
