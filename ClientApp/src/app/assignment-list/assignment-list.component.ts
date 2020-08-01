import { Component, Inject } from '@angular/core';
import {HttpClient, HttpParams} from '@angular/common/http';
import {DateTime} from 'luxon';

@Component({
  selector: 'app-assignment-list',
  templateUrl: './assignment-list.component.html'
})
export class AssignmentListComponent {
  public pageIndex: number;
  public totalPages: number;
  public assignments: AssignmentInfoDto[];
  public currentTime: Date;
  public assignmentColumns = ['id', 'title', 'start', 'end', 'type', 'status', 'registered', 'action'];

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    this.pageIndex = 1; // TODO: get page index from URL
    http.get<AssignmentListPagination>(baseUrl + 'api/v1/assignment', {
      params: new HttpParams().set('page', this.pageIndex.toString())
    }).subscribe(data => {
      this.totalPages = data.totalPages;
      this.assignments = data.items;
      this.currentTime = new Date();
    }, error => console.error(error));
  }

  public canRegisterAssignment(assignment: AssignmentInfoDto) {
    const now = DateTime.local();
    const begin = DateTime.fromISO(assignment.beginTime);
    return assignment.isPublic && now <= begin;
  }

  public canEnterAssignment(assignment: AssignmentInfoDto) {
    const now = DateTime.local();
    const begin = DateTime.fromISO(assignment.beginTime);
    const end = DateTime.fromISO(assignment.endTime);
    return (assignment.isPublic && now >= begin) || now > end;
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
