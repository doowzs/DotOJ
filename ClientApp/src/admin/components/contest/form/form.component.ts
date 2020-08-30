import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';

import { AdminContestService } from '../../../services/contest.service';
import { ContestCreateDto } from '../../../../app/interfaces/contest.interfaces';

@Component({
  selector: 'app-admin-contest-form',
  templateUrl: './form.component.html',
  styleUrls: ['./form.component.css']
})
export class AdminContestFormComponent implements OnInit {
  public form: FormGroup;

  constructor(
    private router: Router,
    private builder: FormBuilder,
    private service: AdminContestService
  ) {
  }

  ngOnInit() {
    this.form = this.builder.group({
      title: [null, [Validators.required, Validators.maxLength(30)]],
      description: [null, [Validators.required, Validators.maxLength(10000)]],
      isPublic: [false, [Validators.required]],
      mode: [null, [Validators.required]],
      period: [null, Validators.required]
    });
  }

  public submitForm(data: any) {
    this.createContest({
      title: data.title,
      description: data.description,
      isPublic: data.isPublic,
      mode: data.mode,
      beginTime: data.period[0],
      endTime: data.period[1]
    });
  }

  public createContest(contest: ContestCreateDto) {
    this.service.createSingle(contest)
      .subscribe(() => {
        this.router.navigate(['/admin/contest']);
      });
  }
}
