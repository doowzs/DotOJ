﻿import {Component, Input, Output, OnInit, EventEmitter} from '@angular/core';
import {FormGroup, Validators, FormControl} from '@angular/forms';
import {DateTime} from 'luxon';

import {AssignmentEditDto} from 'src/interfaces';
import {AdminAssignmentService} from '../assignment.service';

@Component({
  selector: 'app-admin-assignment-form',
  templateUrl: './form.component.html'
})
export class AdminAssignmentFormComponent implements OnInit {
  @Input() public assignment: AssignmentEditDto | null;
  @Output() public created = new EventEmitter<number>();
  @Output() public updated = new EventEmitter<number>();
  @Output() public deleted = new EventEmitter();

  public form: FormGroup;

  constructor(private service: AdminAssignmentService) {
  }

  ngOnInit() {
    const isPublic = this.assignment ? (this.assignment.isPublic ? '1' : '0') : null;
    const mode = this.assignment ? this.assignment.mode.toString() : null;
    this.form = new FormGroup({
      id: new FormControl(this.assignment?.id),
      title: new FormControl(this.assignment?.title, Validators.required),
      description: new FormControl(this.assignment?.description, [Validators.required, Validators.maxLength(1000)]),
      isPublic: new FormControl(isPublic, Validators.required),
      mode: new FormControl(mode, Validators.required),
      beginTime: new FormControl(this.assignment?.beginTime, Validators.required),
      endTime: new FormControl(this.assignment?.endTime, Validators.required)
    }, (g: FormGroup) => {
      const begin = DateTime.fromISO(g.get('beginTime').value);
      const end = DateTime.fromISO(g.get('endTime').value);
      return begin < end ? null : {'invalidPeriod': true};
    });
    if (this.assignment != null) {
      this.form.disable();
    }
  }

  public enableForm() {
    this.form.enable();
  }

  public submitForm() {
    if (this.assignment == null) {
      this.createAssignment();
    } else {
      this.updateAssignment();
    }
  }

  public createAssignment() {
    this.service.CreateSingle({
      id: null,
      title: this.form.get('title').value,
      description: this.form.get('description').value,
      isPublic: Boolean(JSON.parse(this.form.get('isPublic').value)),
      mode: Number(JSON.parse(this.form.get('mode').value)),
      beginTime: this.form.get('beginTime').value,
      endTime: this.form.get('endTime').value
    }).subscribe(data => {
      this.created.emit(data.id);
    }, error => console.error(error));
  }

  public updateAssignment() {
    this.service.UpdateSingle({
      id: this.assignment.id,
      title: this.form.get('title').value,
      description: this.form.get('description').value,
      isPublic: Boolean(JSON.parse(this.form.get('isPublic').value)),
      mode: Number(JSON.parse(this.form.get('mode').value)),
      beginTime: this.form.get('beginTime').value,
      endTime: this.form.get('endTime').value
    }).subscribe(data => {
      this.updated.emit(data.id);
    }, error => console.error(error));
  }

  public deleteAssignment() {
    if (window.confirm('Delete assignment "' + this.assignment.title + '"?')) {
      this.service.DeleteSingle(this.assignment.id)
        .subscribe(() => {
          this.deleted.emit();
        }, error => console.error(error));
    }
  }
}
