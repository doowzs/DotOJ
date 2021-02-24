import { Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import * as moment from 'moment';

import { ContestEditDto } from '../../../../interfaces/contest.interfaces';
import { faCheck } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-admin-contest-form',
  templateUrl: './form.component.html',
  styleUrls: ['./form.component.css']
})
export class AdminContestFormComponent implements OnInit, OnChanges {
  faCheck = faCheck;

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
      beginTime: [null, [Validators.required]],
      endTime: [null, [Validators.required]],
      hasScoreBonus: [null, [Validators.required]],
      scoreBonusTime: [null, []],
      scoreBonusPercentage: [null, [Validators.min(100)]],
      hasScoreDecay: [null, [Validators.required]],
      isScoreDecayLinear: [null, []],
      scoreDecayTime: [null, []],
      scoreDecayPercentage: [null, [Validators.min(0), Validators.max(100)]]
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
        beginTime: this.contest.beginTime as moment.Moment,
        endTime: this.contest.endTime as moment.Moment,
        hasScoreBonus: this.contest.hasScoreBonus.toString(),
        scoreBonusTime: this.contest.scoreBonusTime as moment.Moment,
        scoreBonusPercentage: this.contest.scoreBonusPercentage,
        hasScoreDecay: this.contest.hasScoreDecay.toString(),
        isScoreDecayLinear: this.contest.hasScoreDecay ? this.contest.isScoreDecayLinear.toString() : 'false',
        scoreDecayTime: this.contest.scoreDecayTime as moment.Moment,
        scoreDecayPercentage: this.contest.scoreDecayPercentage
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
      isPublic: data.isPublic === 'true',
      mode: data.mode,
      beginTime: moment(data.beginTime).seconds(0).milliseconds(0).toISOString(),
      endTime: moment(data.endTime).seconds(0).milliseconds(0).toISOString(),
      hasScoreBonus: data.hasScoreBonus === 'true',
      scoreBonusTime: moment(data.scoreBonusTime).seconds(0).milliseconds(0).toISOString(),
      scoreBonusPercentage: data.scoreBonusPercentage,
      hasScoreDecay: data.hasScoreDecay === 'true',
      isScoreDecayLinear: data.hasScoreDecay === 'true' ? (data.isScoreDecayLinear === 'true') : null,
      scoreDecayTime: moment(data.scoreDecayTime).seconds(0).milliseconds(0).toISOString(),
      scoreDecayPercentage: data.scoreDecayPercentage
    });
  }
}
