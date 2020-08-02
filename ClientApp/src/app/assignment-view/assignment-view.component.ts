import {Component} from '@angular/core';

import {
  AssignmentViewDto
} from '../app.interfaces';

@Component({
  selector: 'app-assignment-view',
  templateUrl: './assignment-view.component.html'
})
export class AssignmentViewComponent {
  public assignment: AssignmentViewDto;

  constructor() {
    //
  }
}
