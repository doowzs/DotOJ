import { Component, OnInit, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { interval, Subject, timer } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import * as moment from 'moment';

import { ContestViewDto } from '../../../interfaces/contest.interfaces';
import { ContestService } from '../../../services/contest.service';
import { NzModalService } from 'ng-zorro-antd/modal';

@Component({
  selector: 'app-contest-view',
  templateUrl: './view.component.html',
  styleUrls: ['./view.component.css']
})
export class ContestViewComponent implements OnInit, OnDestroy {
  public contestId: number;
  public contest: ContestViewDto;
  public left: string;
  public ended: boolean;
  public progress: number;
  private complete$ = new Subject();
  private destroy$ = new Subject();

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private service: ContestService,
    private modal: NzModalService
  ) {
    this.contestId = this.route.snapshot.params.contestId;
  }

  ngOnInit() {
    this.service.getSingle(this.contestId)
      .subscribe(contest => {
        this.contest = contest;
        this.ended = moment().isAfter(this.contest.endTime);
        if (this.ended) {
          this.progress = 100;
        } else {
          timer(0, 1000)
            .pipe(takeUntil(this.destroy$), takeUntil(this.complete$))
            .subscribe(() => this.updateProgress());
        }
      });
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private updateProgress() {
    const now = moment();
    const end = this.contest.endTime as moment.Moment;
    if (now.isAfter(end)) {
      this.progress = 100;
      this.complete$.next();
      this.complete$.complete();
      this.modal.success({
        nzTitle: 'Contest ended',
        nzContent: '<p>Contest has ended, refresh page to view results.</p>',
        nzOnOk: () => window.location.reload()
      })
    } else {
      const passed = moment.duration(now.diff(this.contest.beginTime)).asSeconds();
      const total = moment.duration(end.diff(this.contest.beginTime)).asSeconds();
      this.progress = Math.min(100, passed / total * 100);
    }
  }
}
