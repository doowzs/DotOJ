// Read https://ritchiejacobs.be/angular-custom-form-component on how to implement form control.
import { Component, OnInit, Optional, Self } from '@angular/core';
import { ControlValueAccessor, NgControl } from '@angular/forms';
import Vditor from 'vditor';

@Component({
  selector: 'vditor',
  templateUrl: './vditor.component.html',
  styleUrls: ['./vditor.component.css']
})
export class VditorComponent implements OnInit, ControlValueAccessor {
  private vditor: Vditor;
  private disabled: boolean;
  private value: string;
  private input: any;
  private blur: any;

  constructor(@Self() @Optional() private control: NgControl) {
    if (this.control) {
      this.control.valueAccessor = this;
    }
  }

  ngOnInit() {
    this.vditor = new Vditor('vditor', {
      input: this.input,
      blur: this.blur,
      height: 300,
      cache: {
        enable: false
      },
      after: () => {
        // https://github.com/Vanessa219/vditor/issues/273
        if (this.value) {
          this.vditor.setValue(this.value);
        }
        if (this.disabled) {
          this.vditor.disabled();
        }
        this.value = undefined;
        this.disabled = undefined;
      }
    });
  }

  writeValue(value: string) {
    // If vditor is not initialized, save the value first.
    if (this.vditor) {
      this.vditor.setValue(value);
    } else {
      this.value = value;
    }
  }

  registerOnChange(fn: any) {
    this.input = fn;
  }

  registerOnTouched(fn: any) {
    this.blur = fn;
  }

  setDisabledState(isDisabled: boolean) {
    if (this.vditor) {
      if (isDisabled) {
        this.vditor.disabled();
      } else {
        this.vditor.enable();
      }
    } else {
      this.disabled = isDisabled;
    }
  }
}
