import { Component } from '@angular/core';
import { Router } from '@angular/router';

import { AdminProblemService } from '../../../services/problem.service';
import { ProblemEditDto } from '../../../../app/interfaces/problem.interfaces';

@Component({
  selector: 'app-admin-problem-creator',
  templateUrl: './creator.component.html',
  styleUrls: ['./creator.component.css']
})
export class AdminProblemCreatorComponent {
  constructor(
    private router: Router,
    private service: AdminProblemService
  ) {
  }

  public createProblem(problem: ProblemEditDto) {
    this.service.createSingle(problem)
      .subscribe(() => {
        this.router.navigate(['/admin/problem']);
      }, error => console.error(error));
  }
}
