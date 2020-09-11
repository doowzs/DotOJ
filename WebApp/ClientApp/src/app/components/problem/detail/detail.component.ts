import { Component, OnInit, OnDestroy } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { AuthorizeService, IUser } from '../../../../api-authorization/authorize.service';
import { ProblemService } from '../../../services/problem.service';
import { ProblemViewDto } from '../../../interfaces/problem.interfaces';
import { LanguageInfo } from '../../../consts/languages.consts';

@Component({
  selector: 'app-problem-detail',
  templateUrl: './detail.component.html',
  styleUrls: ['./detail.component.css']
})
export class ProblemDetailComponent implements OnInit, OnDestroy {
  public user: IUser;
  public privileged = false;
  public loading = false;
  public problemId: number;
  public problem: ProblemViewDto;
  public language: LanguageInfo;

  public destroy$ = new Subject();

  copyToClipboard = (content: string): void => {
    navigator.clipboard.writeText(content);
  }

  constructor(
    private title: Title,
    private route: ActivatedRoute,
    private service: ProblemService,
    private auth: AuthorizeService
  ) {
    this.route.params
      .pipe(takeUntil(this.destroy$))
      .subscribe(params => {
        this.problemId = params.problemId;
        this.loadProblem();
      });
  }

  ngOnInit() {
    this.auth
      .getUser().subscribe(user => {
      this.user = user;
      this.privileged = user.roles.indexOf('Administrator') >= 0
        || user.roles.indexOf('ContestManager') >= 0;
    })
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  public loadProblem() {
    this.loading = true;
    this.service.getSingle(this.problemId)
      .subscribe(problem => {
        this.problem = problem;
        this.title.setTitle(problem.title);
        this.loading = false;
      });
  }
}

