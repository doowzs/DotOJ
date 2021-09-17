import {Component, OnInit, OnDestroy, ViewChild} from '@angular/core';
import {ActivatedRoute, Router} from '@angular/router';
import {Title} from '@angular/platform-browser';
import {Subject} from 'rxjs';
import {saveAs} from 'file-saver';
import {take, takeUntil} from 'rxjs/operators';
import {Label} from "ng2-charts";
import * as excel from 'exceljs';
import * as moment from 'moment';

import {LanguageInfo} from '../../../../consts/languages.consts';
import {ProblemViewDto} from '../../../../interfaces/problem.interfaces';
import {ContestViewDto} from '../../../../interfaces/contest.interfaces';
import {AuthorizeService, IUser} from '../../../../auth/authorize.service';
import {ProblemService} from '../../../services/problem.service';
import {ContestService} from '../../../services/contest.service';
import {SubmissionService} from "../../../services/submission.service";
import {SubmissionTimelineComponent} from "../../submission/timeline/timeline.component";
import {hackInfo} from "../../../../interfaces/hackScore.interfaces";
import {
  faArrowLeft, faArrowRight,
  faBoxes,
  faCoffee,
  faColumns,
  faCopy, faEdit,
  faFileAlt, faFlask,
  faPaperPlane, faRedo,
  faSdCard,
  faStopwatch,
  faDownload,
  faSyncAlt,
  faTimes
} from '@fortawesome/free-solid-svg-icons';
import {ChartOptions} from "chart.js";

@Component({
  selector: 'app-problem-detail',
  templateUrl: './detail.component.html',
  styleUrls: ['./detail.component.css']
})
export class ProblemDetailComponent implements OnInit, OnDestroy {
  faArrowLeft = faArrowLeft;
  faArrowRight = faArrowRight;
  faDownload = faDownload;
  faBoxes = faBoxes;
  faCoffee = faCoffee;
  faColumns = faColumns;
  faCopy = faCopy;
  faEdit = faEdit;
  faFileAlt = faFileAlt;
  faFlask = faFlask;
  faRedo = faRedo;
  faSdCard = faSdCard;
  faStopwatch = faStopwatch;
  faSyncAlt = faSyncAlt;
  faTimes = faTimes;
  faPaperPlane = faPaperPlane;

  @ViewChild('timeline') timeline: SubmissionTimelineComponent;

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
  public scoreMessage: hackInfo[];
  public activePaneId: number = 1;

  public statsChartLabels: Label[] = [];
  public statsChartData: number[] = [];
  public statsChartOptions: ChartOptions = {
    responsive: true,
    legend: {
      position: 'left'
    }
  }

  public destroy$ = new Subject();

  constructor(
    private title: Title,
    private route: ActivatedRoute,
    private router: Router,
    private contestService: ContestService,
    private problemService: ProblemService,
    private submissionService: SubmissionService,
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
    this.submissionService.newSubmission
      .pipe(takeUntil(this.destroy$))
      .subscribe(submission => this.activePaneId = 2);
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
    this.problemService.getSingle(this.problemId, true)
      .subscribe(problem => {
        this.problem = problem;
        if (this.problem.contestId !== Number(this.contestId)) {
          this.router.navigate(['/contest', this.contestId]);
        }
        this.title.setTitle(problem.title);
        this.loading = false;
        this.statsChartLabels = [];
        this.statsChartData = [];
        for (const verdict in problem.statistics.byVerdict) {
          const value = problem.statistics.byVerdict[verdict];
          this.statsChartLabels.push(verdict + ': ' + value);
          this.statsChartData.push(value);
        }
      });
  }

  public getProblemLabel(problem: ProblemViewDto): string {
    return this.contest.problems.find(p => p.id == problem.id)?.label;
  }

  public updateRoute() {
    const queryParams = this.fullscreen ? {fullscreen: this.fullscreen} : {};
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


  public exportScore() {
    this.problemService.getHackInfo(this.problemId)
      .subscribe(s => {
        this.scoreMessage = s;

        const workbook = new excel.Workbook();
        const sheet = workbook.addWorksheet(this.contest.title);
        sheet.columns = ([
          {header: 'TestId', key: 'id'},
          {header: 'Score', key: 'score'},
        ]);
        for (let data of this.scoreMessage.slice(0)) {
          const row = {
            id: data.test,
            score: data.score
          };
          sheet.addRow(row);
        }
        workbook.xlsx.writeBuffer().then(data => {
          saveAs(new Blob([data]), this.contest.title + '-hackScores.xlsx');
        });
      });
  }
}

