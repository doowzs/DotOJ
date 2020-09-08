import { Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';

import { BulletinEditDto } from '../../../../app/interfaces/bulletin.interfaces';

@Component({
  selector: 'app-admin-bulletin-form',
  templateUrl: './form.component.html',
  styleUrls: ['./form.component.css']
})
export class AdminBulletinFormComponent implements OnInit, OnChanges {
  @Input() public bulletin: BulletinEditDto;
  @Input() public disabled = false;
  @Output() public formSubmit: EventEmitter<BulletinEditDto> = new EventEmitter();

  public form: FormGroup;
  public sampleCaseControls: Array<{ index: number, instances: string[] }> = [];

  constructor(private builder: FormBuilder) {
    this.form = this.builder.group({
      id: [null],
      weight: [null, [Validators.required]],
      content: [null, [Validators.required, Validators.maxLength(30000)]],
      publishAt: [null],
      expireAt: [null]
    });
  }

  ngOnInit() {
    if (this.bulletin) {
      this.form.setValue({
        id: this.bulletin.id,
        weight: this.bulletin.weight,
        content: this.bulletin.content,
        publishAt: (this.bulletin.publishAt as moment.Moment)?.format('YYYY-MM-DD HH:mm') ?? '',
        expireAt: (this.bulletin.expireAt as moment.Moment)?.format('YYYY-MM-DD HH:mm') ?? ''
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
      weight: data.weight,
      content: data.content,
      publishAt: data.publishAt,
      expireAt: data.expireAt
    });
  }
}
