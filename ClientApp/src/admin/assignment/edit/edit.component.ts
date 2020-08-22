import {Component, OnInit} from '@angular/core';
import {AssignmentEditDto} from '../../../interfaces';
import {AdminAssignmentService} from '../assignment.service';
import {ActivatedRoute} from '@angular/router';

@Component({
  selector: 'app-admin-assignment-edit',
  templateUrl: './edit.component.html'
})
export class AdminAssignmentEditComponent implements OnInit {
  public id: number;
  public assignment: AssignmentEditDto;

  constructor(
    private route: ActivatedRoute,
    private service: AdminAssignmentService
  ) {
  }

  ngOnInit() {
    this.id = this.route.snapshot.params.assignmentId;
    this.service.getSingle(this.id).subscribe(assignment => {
      this.assignment = assignment;
    }, error => console.error(error));
  }
}
