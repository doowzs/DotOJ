import { Component, OnDestroy } from '@angular/core';
import { interval, Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import * as moment from 'moment';

import { ApplicationConfigService } from '../../../services/config.service';

@Component({
  selector: 'app-footer-main',
  templateUrl: './main.component.html',
  styleUrls: ['./main.component.css']
})
export class MainFooterComponent implements OnDestroy {
  public title: string;
  public year: string;
  public author: string;
  public now: moment.Moment;
  private destroy$ = new Subject();

  constructor(private config: ApplicationConfigService) {
    this.title = config.title;
    this.year = new Date().getFullYear().toString();
    this.author = config.author;
    this.now = moment().add(config.diff, 'ms');

    interval(1000)
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => this.now.add(1, 's'));
    interval(60000)
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => this.now = moment().add(config.diff, 'ms'));
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
