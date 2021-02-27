import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";

@Injectable({
  providedIn: 'root'
})
export class ApplicationService {

  constructor(private http: HttpClient) {
  }

  public getAverageQueueWaitingTime(): Observable<number> {
    return this.http.get<number>('/application/statistics/average-queue-waiting-time');
  }
}
