import {Component, OnInit} from '@angular/core';
import {ActivatedRoute, Router} from '@angular/router';
import {PageEvent} from '@angular/material/paginator';
import {DateTime} from 'luxon';

import {AssignmentInfoDto} from 'src/interfaces';
import {AssignmentService} from '../assignment.service';

@Component({
  selector: 'app-assignment-list',
  templateUrl: './list.component.html'
})
export class AssignmentListComponent implements OnInit {
  public pageIndex: number;
  public pageSize: number;
  public totalItems: number;
  public assignments: AssignmentInfoDto[];
  public currentTime: Date;
  public assignmentColumns = ['id', 'title', 'start', 'end', 'type', 'status', 'action'];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private service: AssignmentService
  ) {
  }

  ngOnInit() {
    this.pageIndex = this.route.snapshot.queryParams.pageIndex ?? 1;
    this.loadAssignments(this.pageIndex);
  }

  public onPageEvent(event: PageEvent) {
    this.pageIndex = event.pageIndex + 1;
    this.router.navigate(['assignments'], {
      queryParams: {
        pageIndex: this.pageIndex
      }
    }).then(() => {
      this.loadAssignments(this.pageIndex);
    });
  }

  public loadAssignments(pageIndex: number) {
    this.service.getPaginatedList(pageIndex)
      .subscribe(data => {
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

  public canEnterAssignment(assignment: AssignmentInfoDto) {
    const now = DateTime.local();
    const begin = DateTime.fromISO(assignment.beginTime);
    const end = DateTime.fromISO(assignment.endTime);
    return (now >= begin && (assignment.isPublic || assignment.registered)) || now > end;
  }

  public enterAssignment(assignmentId: number) {
    this.router.navigate(['/assignment', assignmentId]);
  }
}


