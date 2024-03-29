﻿import { Component, OnInit, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { interval, Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { VerdictStage } from '../../../../consts/verdicts.consts';
import { PaginatedList } from '../../../../interfaces/pagination.interfaces';
import { SubmissionInfoDto } from '../../../../interfaces/submission.interfaces';
import { AdminSubmissionService } from '../../../services/submission.service';
import { faRedo } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-admin-submission-list',
  templateUrl: './list.component.html',
  styleUrls: ['./list.component.css']
})
export class AdminSubmissionListComponent implements OnInit, OnDestroy {
  faRedo = faRedo;

  public pageIndex: number;
  public list: PaginatedList<SubmissionInfoDto>;
  private destroy$ = new Subject();

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private service: AdminSubmissionService
  ) {
    this.pageIndex = this.route.snapshot.queryParams.pageIndex;
  }

  ngOnInit() {
    this.loadSubmissions();
    interval(2000)
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        if (this.list && this.list.items.filter(s => {
          return s.verdictInfo.stage === VerdictStage.RUNNING ||
            (s.verdictInfo.stage === VerdictStage.REJECTED && s.score == null);
        }).length > 0) {
          this.updatePendingSubmissions();
        }
      });
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  public loadSubmissions() {
    this.service.getPaginatedList(this.pageIndex ?? 1)
      .subscribe(list => this.list = list);
  }

  private updatePendingSubmissions(): void {
    const submissionIds = this.list.items.filter(s => {
      return s.verdictInfo.stage === VerdictStage.RUNNING ||
        (s.verdictInfo.stage === VerdictStage.REJECTED && s.score == null);
    }).map(s => s.id);
    this.service.getBatchInfos(submissionIds)
      .subscribe(updatedSubmissions => {
        for (let i = 0; i < updatedSubmissions.length; ++i) {
          const updated = updatedSubmissions[i];
          const submission = this.list.items.find(s => s.id === updated.id);
          if (submission) {
            submission.isValid = updated.isValid;
            submission.verdict = updated.verdict;
            submission.verdictInfo = updated.verdictInfo;
            submission.time = updated.time;
            submission.memory = updated.memory;
            submission.score = updated.score;
            submission.progress = updated.progress;
            submission.judgedAt = updated.judgedAt;
          }
        }
      });
  }

  public onPageIndexChange(value: number) {
    this.pageIndex = value;
    this.router.navigate(['/admin/submission'], {
      queryParams: {
        pageIndex: value
      }
    });
    this.loadSubmissions();
  }
}
