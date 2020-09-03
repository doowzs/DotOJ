import { Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { ContestEditDto } from '../../../../app/interfaces/contest.interfaces';

@Component({
  selector: 'app-admin-contest-form',
  templateUrl: './form.component.html',
  styleUrls: ['./form.component.css']
})
export class AdminContestFormComponent implements OnInit, OnChanges {
  @Input() public contest: ContestEditDto;
  @Input() public disabled = false;
  @Output() public formSubmit: EventEmitter<ContestEditDto> = new EventEmitter();

  public form: FormGroup;

  constructor(private builder: FormBuilder) {
    this.form = this.builder.group({
      id: [null],
      title: [null, [Validators.required, Validators.maxLength(30)]],
      description: [null, [Validators.required, Validators.maxLength(10000)]],
      isPublic: [null, [Validators.required]],
      mode: [null, [Validators.required]],
      period: [null, Validators.required]
    });
  }

  ngOnInit() {
    if (this.contest) {
      this.form.setValue({
        id: this.contest.id,
        title: this.contest.title,
        description: this.contest.description,
        isPublic: this.contest.isPublic.toString(),
        mode: this.contest.mode.toString(),
        period: [this.contest.beginTime, this.contest.endTime]
      });
    }
    if (this.disabled) {
      this.form.disable();
    }
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes.disabled.currentValue) {
      this.form.disable();
    } else {
      this.form.enable();
    }
  }

  public submitForm(data: any) {
    this.formSubmit.emit({
      id: data.id,
      title: data.title,
      description: data.description,
      isPublic: data.isPublic,
      mode: data.mode,
      beginTime: data.period[0],
      endTime: data.period[1]
    });
  }
}
