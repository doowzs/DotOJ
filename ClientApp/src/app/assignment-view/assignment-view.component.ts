import {Component, Inject} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {ActivatedRoute, Router} from '@angular/router';

import {
  AssignmentViewDto
} from '../app.interfaces';

@Component({
  selector: 'app-assignment-view',
  templateUrl: './assignment-view.component.html'
})
export class AssignmentViewComponent {
  public assignment: AssignmentViewDto;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private http: HttpClient,
    @Inject('BASE_URL') private baseUrl: string
  ) {
    this.loadAssignment(this.route.snapshot.params.assignmentId);
  }

  private loadAssignment(assignmentId: number) {
    this.http.get<AssignmentViewDto>(this.baseUrl + `api/v1/assignment/${assignmentId}`)
      .subscribe(data => this.assignment = data, error => console.error(error));
  }
}
