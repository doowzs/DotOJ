import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { take } from "rxjs/operators";
import * as moment from 'moment';

import { PaginatedList } from '../../../../interfaces/pagination.interfaces';
import { ContestInfoDto } from '../../../../interfaces/contest.interfaces';
import { AuthorizeService } from "../../../../api-authorization/authorize.service";
import { ContestService } from '../../../services/contest.service';
import { ApplicationConfigService } from '../../../services/config.service';
import { faBoxOpen } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-contest-list',
  templateUrl: './list.component.html',
  styleUrls: ['./list.component.css']
})
export class ContestListComponent implements OnInit {
  faBoxOpen = faBoxOpen;

  public privileged = false;
  public list: PaginatedList<ContestInfoDto>;
  public now: moment.Moment;

  constructor(
    private title: Title,
    private auth: AuthorizeService,
    private route: ActivatedRoute,
    private router: Router,
    private service: ContestService,
    private config: ApplicationConfigService
  ) {
    this.title.setTitle('Contests');
    if (!!config.examId) {
      router.navigate(['/']);
    }
  }

  ngOnInit() {
    this.auth.getUser()
      .pipe(take(1))
      .subscribe(user => {
        this.privileged = user.roles.indexOf('Administrator') >= 0
          || user.roles.indexOf('ContestManager') >= 0;
      });
    this.route.queryParams.subscribe(params => {
      this.loadContests(params.pageIndex ?? 1);
    });
  }

  public onPageIndexChange(pageIndex: number) {
    this.router.navigate(['/contests'], {
      queryParams: {pageIndex: pageIndex}
    });
    this.loadContests(pageIndex);
  }

  private loadContests(pageIndex: number): void {
    this.now = moment().add(this.config.diff, 'ms');
    this.service.getPaginatedList(pageIndex).subscribe(list => this.list = list);
  }

  public canEnterContest(contest: ContestInfoDto): boolean {
    return this.privileged || (this.now >= contest.beginTime && (contest.isPublic || contest.registered)) || this.now > contest.endTime;
  }
}
