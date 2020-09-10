import { Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

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
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes.submission) {
      this.form.setValue({
        userId: changes.submission.currentValue.userId,
        problemId: changes.submission.currentValue.problemId,
        language: changes.submission.currentValue.program.languageInfo.name,
        code: changes.submission.currentValue.program.code,
        verdict: changes.submission.currentValue.verdict,
        time: changes.submission.currentValue.time,
        memory: changes.submission.currentValue.memory,
        failedOn: changes.submission.currentValue.failedOn,
        score: changes.submission.currentValue.score,
        message: changes.submission.currentValue.message,
        judgedBy: changes.submission.currentValue.judgedBy,
        judgedAt: changes.submission.currentValue.judgedAt,
        createdAt: changes.submission.currentValue.createdAt
      });
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
  }
}
