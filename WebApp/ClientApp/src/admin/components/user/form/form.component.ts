import { Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { UserEditDto } from '../../../../app/interfaces/user.interfaces';

@Component({
  selector: 'app-admin-user-form',
  templateUrl: './form.component.html',
  styleUrls: ['./form.component.css']
})
export class AdminUserFormComponent implements OnInit, OnChanges {
  @Input() public user: UserEditDto;
  @Input() public disabled = false;
  @Output() public formSubmit: EventEmitter<UserEditDto> = new EventEmitter();

  public form: FormGroup;
  public sampleCaseControls: Array<{ index: number, instances: string[] }> = [];

  constructor(private builder: FormBuilder) {
    this.form = this.builder.group({
      id: [null],
      email: [null, [Validators.email]],
      userName: [null],
      password: [null],
      contestantId: [null, [Validators.required, Validators.maxLength(50)]],
      contestantName: [null, [Validators.required, Validators.maxLength(20)]],
      isAdministrator: [false, [Validators.required]],
      isUserManager: [false, [Validators.required]],
      isContestManager: [false, [Validators.required]],
      isSubmissionManager: [false, [Validators.required]]
    });
  }

  ngOnInit() {
    if (this.user) {
      this.form.setValue({
        id: this.user.id,
        email: this.user.email,
        userName: this.user.userName,
        password: null,
        contestantId: this.user.contestantId,
        contestantName: this.user.contestantName,
        isAdministrator: this.user.isAdministrator,
        isUserManager: this.user.isUserManager,
        isContestManager: this.user.isContestManager,
        isSubmissionManager: this.user.isSubmissionManager
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
    this.form.get('id').disable();
    this.form.get('email').disable();
    this.form.get('userName').disable();
  }

  public submitForm(data: any) {
    this.formSubmit.emit({
      id: data.id,
      email: data.email,
      userName: data.userName,
      password: data.password,
      contestantId: data.contestantId,
      contestantName: data.contestantName,
      isAdministrator: data.isAdministrator,
      isUserManager: data.isUserManager,
      isContestManager: data.isContestManager,
      isSubmissionManager: data.isSubmissionManager
    });
  }
}
