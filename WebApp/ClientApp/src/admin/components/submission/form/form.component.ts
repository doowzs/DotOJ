import { Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import * as moment from 'moment';

import { SubmissionEditDto } from '../../../../app/interfaces/submission.interfaces';
import { Verdicts } from '../../../../app/consts/verdicts.consts';

@Component({
  selector: 'app-admin-submission-form',
  templateUrl: './form.component.html',
  styleUrls: ['./form.component.css']
})
export class AdminSubmissionFormComponent implements OnInit, OnChanges {
  Verdicts = Verdicts;

  @Input() public submission: SubmissionEditDto;
  @Input() public disabled = false;
  @Output() public formSubmit: EventEmitter<SubmissionEditDto> = new EventEmitter();

  public form: FormGroup;
  public sampleCaseControls: Array<{ index: number, instances: string[] }> = [];

  constructor(private builder: FormBuilder) {
    this.form = this.builder.group({
      userId: [null],
      problemId: [null],
      language: [null],
      code: [null],
      verdict: [null, [Validators.required]],
      time: [null],
      memory: [null],
      failedOn: [null],
      score: [null],
      message: [null, [Validators.required, Validators.maxLength(3000)]],
      judgedBy: [null],
      judgedAt: [null],
      createdAt: [null]
    });
  }

  ngOnInit() {
    this.form.setValue({
      userId: this.submission.userId,
      problemId: this.submission.problemId,
      language: this.submission.program.languageInfo.name,
      code: this.submission.program.code,
      verdict: this.submission.verdict,
      time: this.submission.time,
      memory: this.submission.memory,
      failedOn: this.submission.failedOn,
      score: this.submission.score,
      message: this.submission.message,
      judgedBy: this.submission.judgedBy,
      judgedAt: (this.submission.judgedAt as moment.Moment)?.format('YYYY-MM-DD HH:mm') ?? null,
      createdAt: (this.submission.createdAt as moment.Moment)?.format('YYYY-MM-DD HH:mm') ?? null
    });
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes.submission) {
    }
    if (changes.disabled) {
      if (changes.disabled.currentValue) {
        this.form.disable();
      } else {
        this.form.enable();
      }
      this.form.get('userId').disable();
      this.form.get('problemId').disable();
      this.form.get('language').disable();
      this.form.get('code').disable();
      this.form.get('time').disable();
      this.form.get('memory').disable();
      this.form.get('failedOn').disable();
      this.form.get('score').disable();
      this.form.get('judgedBy').disable();
      this.form.get('judgedAt').disable();
      this.form.get('createdAt').disable();
    }
  }

  public submitForm(data: any) {
    this.formSubmit.emit({
      id: this.submission.id,
      userId: null,
      contestantId: null,
      contestantName: null,
      problemId: null,
      program: null,
      verdict: data.verdict,
      verdictInfo: null,
      time: null,
      memory: null,
      failedOn: null,
      score: null,
      message: data.message,
      judgedBy: null,
      judgedAt: null,
      createdAt: null
    });
    this.form.get('judgedAt').setValue(moment().format('YYYY-MM-DD HH:mm'));
  }
}
