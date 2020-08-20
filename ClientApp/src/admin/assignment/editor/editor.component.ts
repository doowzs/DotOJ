import {Component, Input} from '@angular/core';
import {FormGroup, FormBuilder, Validators, FormControl} from '@angular/forms';
import {DateTime} from 'luxon';

import {AssignmentEditDto} from 'src/interfaces';
import {AssignmentService} from '../assignment.service';

@Component({
  selector: 'app-admin-assignment-editor',
  templateUrl: './editor.component.html'
})
export class AssignmentEditorComponent {
  @Input() public assignment: AssignmentEditDto | null;

  public form: FormGroup;

  constructor(private service: AssignmentService) {
    if (this.assignment == null) {
      this.assignment = {} as AssignmentEditDto;
    }
    this.form = new FormGroup({
      id: new FormControl(null),
      title: new FormControl('', Validators.required),
      description: new FormControl('', Validators.maxLength(1000)),
      isPublic: new FormControl(null, Validators.required),
      mode: new FormControl(null, Validators.required),
      beginTime: new FormControl(null, Validators.required),
      endTime: new FormControl(null, Validators.required)
    }, (g: FormGroup) => {
      const begin = DateTime.fromISO(g.get('beginTime').value);
      const end = DateTime.fromISO(g.get('endTime').value);
      return begin < end ? null : {'invalidPeriod': true};
    });
  }

  public submitForm() {
    this.createAssignment();
  }

  public createAssignment() {
    this.service.CreateSingle(this.assignment).subscribe(data => console.log(data), error => console.error(error));
  }

  public updateAssignment() {
  }
}
