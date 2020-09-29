import { Component, OnInit, OnDestroy, SimpleChanges } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { AuthorizeService, IUser } from '../../../../api-authorization/authorize.service';
import { ProblemService } from '../../../services/problem.service';
import { ProblemViewDto } from '../../../interfaces/problem.interfaces';
import { LanguageInfo } from '../../../consts/languages.consts';
import { ContestViewDto } from '../../../interfaces/contest.interfaces';
import { ContestService } from '../../../services/contest.service';
import * as moment from 'moment';
import { faCoffee, faCopy, faPaperPlane, faSdCard, faStopwatch } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-problem-detail',
  templateUrl: './detail.component.html',
  styleUrls: ['./detail.component.css']
})
export class ProblemDetailComponent implements OnInit, OnDestroy {
  faCoffee = faCoffee;
  faCopy = faCopy;
  faSdCard = faSdCard;
  faStopwatch = faStopwatch;
  faPaperPlane = faPaperPlane;

  public user: IUser;
  public loading = true;
  public ended = false;
  public privileged = false;
  public fullscreen = false;
  public contestId: number;
  public contest: ContestViewDto;
  public problemId: number;
  public problem: ProblemViewDto;
  public language: LanguageInfo;

  public destroy$ = new Subject();

  constructor(
    private title: Title,
    private route: ActivatedRoute,
    private router: Router,
    private contestService: ContestService,
    private problemService: ProblemService,
    private auth: AuthorizeService
  ) {
    this.contestId = this.route.snapshot.parent.params.contestId;
  }

  ngOnInit() {
    this.auth
      .getUser().subscribe(user => {
      this.user = user;
      this.privileged = user.roles.indexOf('Administrator') >= 0
        || user.roles.indexOf('ContestManager') >= 0;
    });
    this.contestService.getSingle(this.contestId)
      .subscribe(contest => {
        this.contest = contest;
        this.ended = moment().isAfter(this.contest.endTime);
      });
    this.route.params
      .pipe(takeUntil(this.destroy$))
      .subscribe(params => {
        this.problemId = params.problemId;
        this.loadProblem();
      });
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  public getProblemLabel(problem: ProblemViewDto): string {
    return this.contest.problems.find(p => p.id == problem.id)?.label;
  }

  public onProblemChanged(problemId: number) {
    this.problemId = problemId;
    this.router.navigate(['/contest', this.contestId, 'problem', this.problemId]);
    this.loadProblem();
  }

  public loadProblem() {
    this.loading = true;
    this.problemService.getSingle(this.problemId)
      .subscribe(problem => {
        this.problem = problem;
        this.title.setTitle(problem.title);
        this.loading = false;
      });
  }
}

