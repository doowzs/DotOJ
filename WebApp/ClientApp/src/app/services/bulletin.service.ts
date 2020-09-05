import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { BulletinInfoDto } from '../interfaces/bulletin.interfaces';

@Injectable({
  providedIn: 'root'
})
export class BulletinService {
  constructor(private http: HttpClient) {
  }

  public getList(): Observable<BulletinInfoDto[]> {
    return this.http.get<BulletinInfoDto[]>('/bulletin');
  }
}
