import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import * as moment from 'moment';

import { AuthorizeService } from '../../../../api-authorization/authorize.service';
import { ContestService } from '../../../services/contest.service';
import { ContestViewDto } from '../../../interfaces/contest.interfaces';
import {
  faArrowLeft,
  faBars,
  faCog,
  faIndent,
  faListOl,
  faSignOutAlt, faStream,
  faUser
} from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-header-contest',
  templateUrl: './contest.component.html',
  styleUrls: ['./contest.component.css']
})
export class ContestHeaderComponent implements OnInit {
  faArrowLeft = faArrowLeft;
  faBars = faBars;
  faUser = faUser;
  faSignOutAlt = faSignOutAlt;
  faCog = faCog;
  faIndent = faIndent;
  faListOl = faListOl;
  faStream = faStream;

  public username: Observable<string>;
  public canViewAdminPages: Observable<boolean>;
  public contestId: number;
  public contest: ContestViewDto;
  public ended = false;
  public collapse = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private auth: AuthorizeService,
    private service: ContestService
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
      });
  }
}
