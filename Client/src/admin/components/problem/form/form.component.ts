import { Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, ValidationErrors, Validators } from '@angular/forms';

import { Languages } from '../../../../consts/languages.consts';
import { ProblemEditDto, ProblemType, TestCase } from '../../../../interfaces/problem.interfaces';
import { faCheck, faPlus, faTimes } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-admin-problem-form',
  templateUrl: './form.component.html',
  styleUrls: ['./form.component.css']
})
export class AdminProblemFormComponent implements OnInit, OnChanges {
  faCheck = faCheck;
  faPlus = faPlus;
  faTimes = faTimes;

  @Input() public problem: ProblemEditDto;
  @Input() public disabled = false;
  @Output() public formSubmit: EventEmitter<ProblemEditDto> = new EventEmitter();

  public form: FormGroup;
  public sampleCaseControls: Array<{ index: number, instances: string[] }> = [];

  constructor(private builder: FormBuilder) {
    this.form = this.builder.group({
      id: [null],
      contestId: [null, [Validators.required]],
      type: [null, [Validators.required]],
      title: [null, [Validators.required, Validators.maxLength(30)]],
      description: [null, [Validators.required, Validators.maxLength(10000)]],
      inputFormat: [null, [Validators.maxLength(10000)]],
      outputFormat: [null, [Validators.maxLength(10000)]],
      footNote: [null, [Validators.maxLength(10000)]],
      timeLimit: [1000, [Validators.min(100), Validators.max(30000)]],
      memoryLimit: [256000, [Validators.min(1000), Validators.max(512000)]],
      hasSpecialJudge: [null, []],
      specialJudgeCode: [null, [Validators.maxLength(30720)]]
    }, {
      validators: [
        (control: FormGroup): ValidationErrors | null => {
          if (control.value.type === '0') {
            if (!control.value.inputFormat) return {inputFormat: true};
            if (!control.value.outputFormat) return {outputFormat: true};
            if (control.value.timeLimit.length === 0) return {timeLimit: true};
            if (!control.value.memoryLimit) return {memoryLimit: true};
            if (control.value.hasSpecialJudge === null) return {hasSpecialJudge: true};
            if (control.value.hasSpecialJudge === 'true' && !control.value.specialJudgeCode) {
              return {specialJudgeCode: true};
            }
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
        type: this.problem.type.toString(),
        title: this.problem.title,
        description: this.problem.description,
        inputFormat: this.problem.inputFormat,
        outputFormat: this.problem.outputFormat,
        footNote: this.problem.footNote,
        timeLimit: this.problem.timeLimit ?? 1000,
        memoryLimit: this.problem.memoryLimit ?? 256000,
        hasSpecialJudge: this.problem.hasSpecialJudge.toString() ?? 'false',
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
    this.sampleCaseControls.push({index: index, instances: instances});
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
    if (data.type === ProblemType.Ordinary.toString()) {
      this.formSubmit.emit({
        id: data.id,
        contestId: data.contestId,
        type: ProblemType.Ordinary,
        title: data.title,
        description: data.description,
        inputFormat: data.inputFormat,
        outputFormat: data.outputFormat,
        footNote: data.footNote,
        timeLimit: data.timeLimit,
        memoryLimit: data.memoryLimit,
        hasSpecialJudge: data.hasSpecialJudge === 'true',
        specialJudgeProgram: data.hasSpecialJudge === 'true' ? {
          language: Languages.find(l => l.name === 'C++ 17').code,
          code: btoa(data.specialJudgeCode)
        } : null,
        hasHacking: false,
        sampleCases: sampleCases
      });
    } else {
      this.formSubmit.emit({
        id: data.id,
        contestId: data.contestId,
        type: ProblemType.TestKitLab,
        title: data.title,
        description: data.description,
        inputFormat: "N/A",
        outputFormat: "N/A",
        footNote: null,
        timeLimit: 1000,
        memoryLimit: 256000,
        hasSpecialJudge: false,
        specialJudgeProgram: null,
        hasHacking: false,
        sampleCases: []
      });
    }
  }
}
