import {Component, Input} from '@angular/core';

@Component({
  selector: 'app-admin-assignment-creator',
  templateUrl: './create.component.html'
})
// TODO: change name to assignment editor
export class AssignmentCreatorComponent {
  @Input() public assignmentId: number | null;

  constructor() {
  }

  public submitForm() {
    // TODO: implement this action
  }

  public createAssignment() {
  }

  public updateAssignment() {
  }
}
