import {Component, Inject} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {ActivatedRoute, Router} from '@angular/router';

import {
  AssignmentViewDto, ProblemViewDto
} from '../app.interfaces';

@Component({
  selector: 'app-assignment-view',
  templateUrl: './assignment-view.component.html'
})
export class AssignmentViewComponent {
  public assignment: AssignmentViewDto;
  public problemColumns = ['label', 'title', 'action'];

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
      .subscribe(data => {
        this.assignment = data;
        for (let i = 0; i < this.assignment.problems.length; ++i) {
          this.assignment.problems[i].label = String.fromCharCode('A'.charCodeAt(0) + i);
        }
        console.log(this.assignment.problems);
      }, error => console.error(error));
  }
}
