import {Component, Inject} from '@angular/core';
import {HttpClient, HttpParams} from '@angular/common/http';
import {ActivatedRoute, Router} from '@angular/router';
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

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private http: HttpClient,
    @Inject('BASE_URL') private baseUrl: string
  ) {
    this.pageIndex = this.route.snapshot.queryParams.pageIndex ?? 1;
    this.loadAssignments(this.pageIndex);
  }

  public onPageEvent(event: PageEvent) {
    this.pageIndex = event.pageIndex + 1;
    this.router.navigate(['/assignments'], {
      queryParams: {
        pageIndex: this.pageIndex
      }
    }).then(() => {
      this.loadAssignments(this.pageIndex);
    });
  }

  public loadAssignments(pageIndex: number) {
    this.http.get<AssignmentListPagination>(this.baseUrl + 'api/v1/assignment', {
      params: new HttpParams().set('pageIndex', pageIndex.toString())
    }).subscribe(data => {
      this.pageSize = data.pageSize;
      this.totalItems = data.totalItems;
      this.assignments = data.items;
      this.currentTime = new Date();
    }, error => console.error(error));
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


