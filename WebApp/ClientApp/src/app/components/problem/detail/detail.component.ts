import { Component, OnInit, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { Subject } from 'rxjs';
import { take, takeUntil } from 'rxjs/operators';
import * as moment from 'moment';

import { LanguageInfo } from '../../../../consts/languages.consts';
import { ProblemViewDto } from '../../../../interfaces/problem.interfaces';
import { ContestViewDto } from '../../../../interfaces/contest.interfaces';
import { AuthorizeService, IUser } from '../../../../api-authorization/authorize.service';
import { ProblemService } from '../../../services/problem.service';
import { ContestService } from '../../../services/contest.service';
import {
  faBoxes,
  faCoffee,
  faColumns,
  faCopy, faEdit,
  faPaperPlane, faRedo,
  faSdCard,
  faStopwatch,
  faTimes
} from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-problem-detail',
  templateUrl: './detail.component.html',
  styleUrls: ['./detail.component.css']
})
export class ProblemDetailComponent implements OnInit, OnDestroy {
  faBoxes = faBoxes;
  faCoffee = faCoffee;
  faColumns = faColumns;
  faCopy = faCopy;
  faEdit = faEdit;
  faRedo = faRedo;
  faSdCard = faSdCard;
  faStopwatch = faStopwatch;
  faTimes = faTimes;
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
    this.fullscreen = this.route.snapshot.queryParams.fullscreen;
  }

  ngOnInit() {
    this.auth.getUser()
      .pipe(take(1))
      .subscribe(user => {
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

  public loadProblem() {
    this.loading = true;
    this.problemService.getSingle(this.problemId)
      .subscribe(problem => {
        this.problem = problem;
        this.title.setTitle(problem.title);
        this.loading = false;
      });
  }

  public getProblemLabel(problem: ProblemViewDto): string {
    return this.contest.problems.find(p => p.id == problem.id)?.label;
  }

  public updateRoute() {
    const queryParams = this.fullscreen ? { fullscreen: this.fullscreen } : {};
    this.router.navigate(['/contest', this.contestId, 'problem', this.problemId], {
      replaceUrl: true,
      queryParams: queryParams
    });
  }

  public changeProblem(problemId: number) {
    this.problemId = problemId;
    this.updateRoute();
    this.loadProblem();
  }

  public toggleFullscreen() {
    this.fullscreen = !this.fullscreen;
    this.updateRoute();
  }
}

