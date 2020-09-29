import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Title } from '@angular/platform-browser';
import * as moment from 'moment';

import { ContestViewDto } from '../../../../interfaces/contest.interfaces';
import { AuthorizeService, IUser } from '../../../../api-authorization/authorize.service';
import { ContestService } from '../../../services/contest.service';
import { faBoxOpen, faCheck, faEdit } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-contest-description',
  templateUrl: './description.component.html',
  styleUrls: ['./description.component.css']
})
export class ContestDescriptionComponent implements OnInit {
  faBoxOpen = faBoxOpen;
  faCheck = faCheck;
  faEdit = faEdit;

  public user: IUser;
  public privileged = false;
  public contestId: number;
  public contest: ContestViewDto;
  public ended: boolean;

  constructor(
    private title: Title,
    private route: ActivatedRoute,
    private service: ContestService,
    private auth: AuthorizeService
  ) {
    this.contestId = this.route.snapshot.params.contestId;
  }

  ngOnInit() {
    this.auth.getUser().subscribe(user => {
      this.user = user;
      this.privileged = user.roles.indexOf('Administrator') >= 0
        || user.roles.indexOf('ContestManager') >= 0;
    })
    this.service.getSingle(this.contestId)
      .subscribe(contest => {
        this.contest = contest;
        this.ended = moment().isAfter(this.contest.endTime);
        this.title.setTitle(contest.title);
      });
  }
}
