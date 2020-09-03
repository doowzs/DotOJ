import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable()
export class ApplicationConfigService {
  public title: string;
  public author: string;
  public messageOfTheDay: string;

  constructor(private http: HttpClient) {
  }

  loadApplicationConfig(): Promise<any> {
    return this.http.get('/application/config')
      .toPromise()
      .then(data => {
        Object.assign(this, data);
        return data;
      });
  }
}
