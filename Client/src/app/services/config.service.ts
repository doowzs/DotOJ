import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import * as moment from 'moment';

@Injectable()
export class ApplicationConfigService {
  public title: string;
  public author: string;
  public version: string;
  public examId: number;
  public serverTime: string;
  public diff: number;

  constructor(private http: HttpClient) {
  }

  loadApplicationConfig(): Promise<any> {
    return this.http.get('/application/config')
      .toPromise()
      .then(data => {
        Object.assign(this, data);
        this.diff = moment(this.serverTime).local().diff(moment.now());
        return data;
      });
  }
}
