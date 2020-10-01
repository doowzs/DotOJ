import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { interval, Observable, Subject, timer } from 'rxjs';
import { map, take, takeUntil } from 'rxjs/operators';
import * as moment from 'moment';

import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { ContestViewDto } from '../../../../interfaces/contest.interfaces';
import { AuthorizeService } from '../../../../api-authorization/authorize.service';
import { ApplicationConfigService } from '../../../services/config.service';
import { ContestService } from '../../../services/contest.service';
import {
  faArrowLeft,
  faBars, faClock,
  faCog,
  faPaperPlane,
  faSignOutAlt, faStream, faTools, faTrophy,
  faUser
} from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-header-contest',
  templateUrl: './contest.component.html',
  styleUrls: ['./contest.component.css']
})
export class ContestHeaderComponent implements OnInit, OnDestroy {
  faArrowLeft = faArrowLeft;
  faBars = faBars;
  faClock = faClock;
  faCog = faCog;
  faPaperPlane = faPaperPlane;
  faSignOutAlt = faSignOutAlt;
  faStream = faStream;
  faTools = faTools;
  faTrophy = faTrophy;
  faUser = faUser;

  @ViewChild('contestEndedModal') contestEndedModal;

  public username: Observable<string>;
  public canViewAdminPages: Observable<boolean>;
  public contestId: number;
  public contest: ContestViewDto;
  public now: moment.Moment;
  public ended = false;
  public progress = 0;
  public collapse = true;

  private complete$ = new Subject();
  private destroy$ = new Subject();

  constructor(
    public route: ActivatedRoute,
    private router: Router,
    private config: ApplicationConfigService,
    private auth: AuthorizeService,
    private service: ContestService,
    private modal: NgbModal,
  ) {
    this.username = this.auth.getUser().pipe(take(1), map(u => u && u.name));
    this.canViewAdminPages = this.auth.getUser().pipe(take(1), map(u => u && u.roles.length > 0));
    this.contestId = this.route.snapshot.params.contestId;
    this.now = moment().add(config.diff, 'ms');

    interval(1000)
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => this.now.add(1, 's'));
    interval(60000)
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => this.now = moment().add(config.diff, 'ms'));
  }

  ngOnInit() {
    this.service.getSingle(this.contestId)
      .subscribe(contest => {
        this.contest = contest;
        this.ended = this.now.isAfter(this.contest.endTime);
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
    const end = this.contest.endTime as moment.Moment;
    if (this.now.isAfter(end)) {
      this.progress = 100;
      this.complete$.next();
      this.complete$.complete();
      this.modal.open(this.contestEndedModal).result.then(() => window.location.reload());
    } else {
      const passed = moment.duration(this.now.diff(this.contest.beginTime)).asSeconds();
      const total = moment.duration(end.diff(this.contest.beginTime)).asSeconds();
      this.progress = Math.min(100, passed / total * 100);
    }
  }
}
