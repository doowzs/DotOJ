import {Component, Inject} from '@angular/core';
import {HttpClient, HttpParams} from '@angular/common/http';
import {Router} from '@angular/router';
import {PageEvent} from '@angular/material/paginator';
import {DateTime} from 'luxon';

import {
  AssignmentInfoDto,
  AssignmentListPagination
} from '../app.interfaces';

@Component({
  selector: 'app-assignment-list',
  templateUrl: './assignment-list.component.html'
})
export class AssignmentListComponent {
  public pageIndex: number;
  public pageSize: number;
  public totalItems: number;
  public assignments: AssignmentInfoDto[];
  public currentTime: Date;
  public assignmentColumns = ['id', 'title', 'start', 'end', 'type', 'status', 'registered', 'action'];

  constructor(private router: Router, http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    this.pageIndex = 1; // TODO: get page index from URL
    http.get<AssignmentListPagination>(baseUrl + 'api/v1/assignment', {
      params: new HttpParams().set('page', this.pageIndex.toString())
    }).subscribe(data => {
      this.pageSize = data.pageSize;
      this.totalItems = data.totalItems;
      this.assignments = data.items;
      this.currentTime = new Date();
    }, error => console.error(error));
  }

  public onPageEvent(event: PageEvent) {
    this.pageIndex = event.pageIndex;
    console.log(this.router.getCurrentNavigation());
  }

  public isAssignmentPending(assignment: AssignmentInfoDto) {
    return DateTime.local() < DateTime.fromISO(assignment.beginTime);
  }

  public isAssignmentEnded(assignment: AssignmentInfoDto) {
    return DateTime.local() > DateTime.fromISO(assignment.endTime);
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

  public enterAssignment(assignmentId: number) {
    this.router.navigate(['/assignment', assignmentId]);
  }
}


