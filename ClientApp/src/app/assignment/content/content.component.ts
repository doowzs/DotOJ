import {Component} from '@angular/core';
import {ActivatedRoute, Router} from '@angular/router';

import {AssignmentService} from '../assignment.service';
import {AssignmentViewDto} from '../../app.interfaces';

@Component({
  selector: 'app-assignment-content',
  templateUrl: './content.component.html'
})
export class AssignmentContentComponent {
  public assignment: AssignmentViewDto;
  public problemColumns = ['label', 'title', 'action'];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private service: AssignmentService
  ) {
    this.service.getSingle(this.route.snapshot.params.assignmentId)
      .subscribe(assignment => {
        this.assignment = assignment;
      });
  }
}
