import {Component, Input} from '@angular/core';
import {ActivatedRoute, Router} from '@angular/router';
import {MatSelectChange} from '@angular/material/select';
import {MatSnackBar} from '@angular/material/snack-bar';

import {
  AssignmentViewDto,
  ProblemViewDto,
  TestCaseDto
} from 'src/interfaces';

@Component({
  selector: 'app-problem-content',
  templateUrl: './content.component.html'
})
export class ProblemContentComponent {
  @Input() public assignment: AssignmentViewDto;
  @Input() public problem: ProblemViewDto;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private snackBar: MatSnackBar
  ) {
  }

  public changeProblem(event: MatSelectChange) {
    if (event.value !== this.problem.id) {
      this.router.navigate(['assignment', this.assignment.id, 'problem', event.value]);
    }
  }

  public copyTestCase(testCase: TestCaseDto) {
    navigator.clipboard.writeText(testCase.input)
      .then(() => this.snackBar.open('Copied to clipboard.', 'Done', {
        duration: 2000,
        horizontalPosition: 'right'
      }));
  }
}
