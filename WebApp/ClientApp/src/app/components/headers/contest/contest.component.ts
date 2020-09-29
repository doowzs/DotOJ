import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Observable, Subject, timer } from 'rxjs';
import { map, takeUntil } from 'rxjs/operators';
import * as moment from 'moment';

import { AuthorizeService } from '../../../../api-authorization/authorize.service';
import { ContestService } from '../../../services/contest.service';
import { ContestViewDto } from '../../../interfaces/contest.interfaces';
import {
  faArrowLeft,
  faBars,
  faCog,
  faIndent,
  faListOl, faPaperPlane,
  faSignOutAlt, faStream, faTrophy,
  faUser
} from '@fortawesome/free-solid-svg-icons';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';

@Component({
  selector: 'app-header-contest',
  templateUrl: './contest.component.html',
  styleUrls: ['./contest.component.css']
})
export class ContestHeaderComponent implements OnInit, OnDestroy {
  faArrowLeft = faArrowLeft;
  faBars = faBars;
  faUser = faUser;
  faSignOutAlt = faSignOutAlt;
  faCog = faCog;
  faPaperPlane = faPaperPlane;
  faTrophy = faTrophy;
  faStream = faStream;

  @ViewChild('contestEndedModal') contestEndedModal;

  public username: Observable<string>;
  public canViewAdminPages: Observable<boolean>;
  public contestId: number;
  public contest: ContestViewDto;
  public ended = false;
  public progress = 0;
  public collapse = true;

  private complete$ = new Subject();
  private destroy$ = new Subject();

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private auth: AuthorizeService,
    private service: ContestService,
    private modal: NgbModal,
  ) {
    this.username = this.auth.getUser().pipe(map(u => u && u.name));
    this.canViewAdminPages = this.auth.getUser().pipe(map(u => u && u.roles.length > 0));
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
      this.modal.open(this.contestEndedModal).result.then(() => window.location.reload());
    } else {
      const passed = moment.duration(now.diff(this.contest.beginTime)).asSeconds();
      const total = moment.duration(end.diff(this.contest.beginTime)).asSeconds();
      this.progress = Math.min(100, passed / total * 100);
    }
  }
}
