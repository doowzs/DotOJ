import {Component, Input} from '@angular/core';

import {AssignmentEditDto} from 'src/interfaces';
import {AssignmentService} from '../assignment.service';

@Component({
  selector: 'app-admin-assignment-editor',
  templateUrl: './editor.component.html'
})
// TODO: change name to assignment editor
export class AssignmentEditorComponent {
  @Input() public assignmentId: number | null;
  @Input() public assignment: AssignmentEditDto;

  constructor(private service: AssignmentService) {
    if (this.assignment == null) {
      this.assignment = {} as AssignmentEditDto;
    }
  }

  public submitForm() {
    // TODO: implement this action
  }

  public createAssignment() {
    this.service.CreateSingle(this.assignment);
  }

  public updateAssignment() {
  }
}
