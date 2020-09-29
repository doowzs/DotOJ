import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import { ProblemEditDto } from '../../../../interfaces/problem.interfaces';
import { AdminProblemService } from '../../../services/problem.service';

@Component({
  selector: 'app-admin-problem-editor',
  templateUrl: './editor.component.html',
  styleUrls: ['./editor.component.css']
})
export class AdminProblemEditorComponent implements OnInit {
  public edit: boolean;
  public problemId: number;
  public problem: ProblemEditDto;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private service: AdminProblemService
  ) {
    this.edit = this.route.snapshot.queryParams.edit ?? false;
    this.problemId = this.route.snapshot.parent.params.problemId;
  }

  ngOnInit() {
    this.service.getSingle(this.problemId)
      .subscribe(problem => this.problem = problem);
  }

  public editProblem() {
    this.edit = true;
    this.router.navigate(['/admin/problem', this.problemId], {
      queryParams: { edit: true }
    });
  }

  public updateProblem(problem: ProblemEditDto) {
    this.service.updateSingle(problem).subscribe(() => {
      this.edit = false;
      this.router.navigate(['/admin/problem', this.problemId]);
    }, error => console.error(error));
  }

  public deleteProblem() {
    this.service.deleteSingle(this.problemId).subscribe(() => {
      this.router.navigate(['/admin/problem']);
    }, error => console.error(error));
  }
}
