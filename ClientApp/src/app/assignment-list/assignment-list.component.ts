import { Component, Inject } from '@angular/core';
import {HttpClient, HttpParams} from '@angular/common/http';

@Component({
  selector: 'app-assignment-list',
  templateUrl: './assignment-list.component.html'
})
export class AssignmentListComponent {
  public pageIndex: number;
  public totalPages: number;
  public assignments: AssignmentInfoDto[];

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    this.pageIndex = 1; // TODO: get page index from URL
    http.get<AssignmentListPagination>(baseUrl + 'api/v1/assignment', {
      params: new HttpParams().set('page', this.pageIndex.toString())
    }).subscribe(data => {
      this.totalPages = data.totalPages;
      this.assignments = data.items;
      console.log(this.assignments);
    }, error => console.error(error));
  }
}

interface AssignmentListPagination {
  pageIndex: number;
  totalPages: number;
  items: AssignmentInfoDto[];
}

interface AssignmentInfoDto {
  id: number;
  title: string;
  isPublic: boolean;
  mode: number;
  beginTime: Date;
  endTime: Date;
  registered: number;
}
