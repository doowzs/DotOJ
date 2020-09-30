import { Component, Optional, Self, ViewChild } from '@angular/core';
import { ControlValueAccessor, NgControl } from '@angular/forms';
import * as moment from 'moment';
import { NgbDatepicker } from '@ng-bootstrap/ng-bootstrap';

@Component({
  selector: 'datetime',
  templateUrl: './datetime.component.html',
  styleUrls: ['./datetime.component.css']
})
export class DatetimeComponent implements ControlValueAccessor {
  public value: string;
  public disabled: boolean;
  private input: any;
  private blur: any;

  @ViewChild('picker') picker: NgbDatepicker;

  constructor(@Self() @Optional() private control: NgControl) {
    if (this.control) {
      this.control.valueAccessor = this;
    }
  }

  updateValue() {
    let v = moment(this.value, 'YYYY-MM-DD HH:mm', true);
    if (!v.isValid()) {
      v = null;
    }
    if (this.input) this.input(v);
    if (this.blur) this.blur(v);
  }

  writeValue(value: moment.Moment) {
    this.value = value.format('YYYY-MM-DD HH:mm');
  }

  registerOnChange(fn: any) {
    this.input = fn;
  }

  registerOnTouched(fn: any) {
    this.blur = fn;
  }

  setDisabledState(isDisabled: boolean) {
    this.disabled = isDisabled;
  }
}
