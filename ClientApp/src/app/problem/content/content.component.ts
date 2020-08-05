import {Component, Input} from '@angular/core';
import {ActivatedRoute, Router} from '@angular/router';
import {AssignmentViewDto, ProblemViewDto} from '../../app.interfaces';

@Component({
  selector: 'app-problem-content',
  templateUrl: './content.component.html'
})
export class ProblemContentComponent {
  @Input() public assignment: AssignmentViewDto;
  @Input() public problem: ProblemViewDto;

  constructor(
    private route: ActivatedRoute,
    private router: Router
  ) {
  }
}
