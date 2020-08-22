import {Component, OnInit} from '@angular/core';
import {AssignmentEditDto} from '../../../interfaces';
import {AdminAssignmentService} from '../assignment.service';
import {ActivatedRoute, Router} from '@angular/router';
import {MatSnackBar} from '@angular/material/snack-bar';

@Component({
  selector: 'app-admin-assignment-edit',
  templateUrl: './edit.component.html'
})
export class AdminAssignmentEditComponent implements OnInit {
  public id: number;
  public assignment: AssignmentEditDto;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private service: AdminAssignmentService,
    private snackBar: MatSnackBar
  ) {
  }

  ngOnInit() {
    this.id = this.route.snapshot.params.assignmentId;
    this.service.getSingle(this.id).subscribe(assignment => {
      this.assignment = assignment;
    }, error => console.error(error));
  }

  public updated(id: number) {
    this.snackBar.open(`Assignment ${id} updated.`, 'OK');
  }

  public deleted() {
    this.snackBar.open('Assignment deleted.', 'OK');
    this.router.navigate(['/admin/assignment']);
  }
}
