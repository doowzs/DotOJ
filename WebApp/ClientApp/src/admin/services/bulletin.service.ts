import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import * as moment from 'moment';

import { BulletinEditDto, BulletinInfoDto } from '../../app/interfaces/bulletin.interfaces';
import { PaginatedList } from '../../app/interfaces/pagination.interfaces';

@Injectable({
  providedIn: 'root'
})
export class AdminBulletinService {
  constructor(private http: HttpClient) {
  }

  public getPaginatedList(pageIndex: number): Observable<PaginatedList<BulletinInfoDto>> {
    return this.http.get<PaginatedList<BulletinInfoDto>>('/admin/bulletin', {
      params: new HttpParams().set('pageIndex', pageIndex.toString())
    }).pipe(map(list => {
      for (let i = 0; i < list.items.length; ++i) {
        const bulletin = list.items[i];
        if (bulletin.publishAt) {
          bulletin.publishAt = moment.utc(bulletin.publishAt).local();
        }
        if (bulletin.expireAt) {
          bulletin.expireAt = moment.utc(bulletin.expireAt).local();
        }
      }
      return list;
    }));
  }

  public getSingle(bulletinId: number): Observable<BulletinEditDto> {
    return this.http.get<BulletinEditDto>('/admin/bulletin/' + bulletinId.toString())
      .pipe(map(data => {
        if (data.publishAt) {
          data.publishAt = moment.utc(data.publishAt).local();
        }
        if (data.expireAt) {
          data.expireAt = moment.utc(data.expireAt).local();
        }
        return data;
      }));
  }

  public createSingle(bulletin: BulletinEditDto): Observable<any> {
    return this.http.post('/admin/bulletin', bulletin);
  }

  public updateSingle(bulletin: BulletinEditDto): Observable<BulletinEditDto> {
    return this.http.put<BulletinEditDto>('/admin/bulletin/' + bulletin.id.toString(), bulletin);
  }

  public deleteSingle(bulletinId: number): Observable<any> {
    return this.http.delete('/admin/bulletin/' + bulletinId.toString());
  }
}
