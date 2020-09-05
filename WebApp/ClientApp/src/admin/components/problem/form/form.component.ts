import { Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, ValidationErrors, Validators } from '@angular/forms';

import { ProblemEditDto, TestCase } from '../../../../app/interfaces/problem.interfaces';
import { Languages } from '../../../../app/consts/languages.consts';

@Component({
  selector: 'app-admin-problem-form',
  templateUrl: './form.component.html',
  styleUrls: ['./form.component.css']
})
export class AdminProblemFormComponent implements OnInit, OnChanges {
  @Input() public problem: ProblemEditDto;
  @Input() public disabled = false;
  @Output() public formSubmit: EventEmitter<ProblemEditDto> = new EventEmitter();

  public form: FormGroup;
  public sampleCaseControls: Array<{ index: number, instances: string[] }> = [];

  constructor(private builder: FormBuilder) {
    this.form = this.builder.group({
      id: [null],
      contestId: [null, [Validators.required]],
      title: [null, [Validators.required, Validators.maxLength(30)]],
      description: [null, [Validators.required, Validators.maxLength(10000)]],
      inputFormat: [null, [Validators.required, Validators.maxLength(10000)]],
      outputFormat: [null, [Validators.required, Validators.maxLength(10000)]],
      footNote: [null, [Validators.maxLength(10000)]],
      timeLimit: [null, [Validators.required, Validators.min(100), Validators.max(30000)]],
      memoryLimit: [null, [Validators.required, Validators.min(1000), Validators.max(512000)]],
      hasSpecialJudge: [null, [Validators.required]],
      specialJudgeCode: [null, [Validators.maxLength(30720)]]
    }, {
      validators: [
        (control: FormGroup): ValidationErrors | null => {
          if (control.value.hasSpecialJudge === 'true' && !control.value.specialJudgeCode) {
            return { specialJudgeCode: true };
          }
          return null;
        }
      ]
    });
  }

  ngOnInit() {
    if (this.problem) {
      this.form.setValue({
        id: this.problem.id,
        contestId: this.problem.contestId,
        title: this.problem.title,
        description: this.problem.description,
        inputFormat: this.problem.inputFormat,
        outputFormat: this.problem.outputFormat,
        footNote: this.problem.footNote,
        timeLimit: this.problem.timeLimit,
        memoryLimit: this.problem.memoryLimit,
        hasSpecialJudge: this.problem.hasSpecialJudge.toString(),
        specialJudgeCode: this.problem.specialJudgeProgram?.code ?? ''
      });
      for (let i = 0; i < this.problem.sampleCases.length; ++i) {
        this.addSampleCase(this.problem.sampleCases[i].input, this.problem.sampleCases[i].output);
      }
    } else {
      this.addSampleCase();
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

  public addSampleCase(input: string = null, output: string = null) {
    const index = this.sampleCaseControls.length > 0 ? this.sampleCaseControls[this.sampleCaseControls.length - 1].index + 1 : 0;
    const instances = [`sampleCaseInput${index}`, `sampleCaseOutput${index}`];
    this.sampleCaseControls.push({ index: index, instances: instances });
    this.form.addControl(instances[0], new FormControl(input, [Validators.required, Validators.maxLength(10000)]));
    this.form.addControl(instances[1], new FormControl(output, [Validators.required, Validators.maxLength(10000)]));
  }

  public removeSampleCase(control: { index: number, instances: string[] }) {
    if (this.sampleCaseControls.length > 1) {
      this.form.removeControl(control.instances[0]);
      this.form.removeControl(control.instances[1]);
      this.sampleCaseControls.splice(this.sampleCaseControls.indexOf(control), 1);
    }
  }

  public submitForm(data: any) {
    const sampleCases: TestCase[] = [];
    for (let i = 0; i < this.sampleCaseControls.length; ++i) {
      const control = this.sampleCaseControls[i];
      sampleCases.push({
        input: this.form.get(control.instances[0]).value,
        output: this.form.get(control.instances[1]).value
      });
    }
    this.formSubmit.emit({
      id: data.id,
      contestId: data.contestId,
      title: data.title,
      description: data.description,
      inputFormat: data.inputFormat,
      outputFormat: data.outputFormat,
      footNote: data.footNote,
      timeLimit: data.timeLimit,
      memoryLimit: data.memoryLimit,
      hasSpecialJudge: data.hasSpecialJudge === 'true',
      specialJudgeProgram: data.hasSpecialJudge ? {
        language: Languages.find(l => l.name === 'C++').code,
        code: btoa(data.specialJudgeCode)
      } : null,
      hasHacking: false,
      sampleCases: sampleCases
    });
  }
}
