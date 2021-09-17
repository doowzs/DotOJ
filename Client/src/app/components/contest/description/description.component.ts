import {Component, OnInit} from '@angular/core';
import {ActivatedRoute} from '@angular/router';
import {Title} from '@angular/platform-browser';
import {saveAs} from 'file-saver';
import * as moment from 'moment';
import * as excel from 'exceljs';

import {ContestViewDto} from '../../../../interfaces/contest.interfaces';
import {AuthorizeService, IUser} from '../../../../auth/authorize.service';
import {ContestService} from '../../../services/contest.service';
import {
  faArrowAltCircleDown,
  faArrowAltCircleUp,
  faDownload,
  faBoxOpen,
  faCheck,
  faClipboardCheck,
  faEdit,
  faTimes,
  faUser
} from '@fortawesome/free-solid-svg-icons';
import {take} from 'rxjs/operators';
import {ApplicationConfigService} from "../../../services/config.service";
import {SubmissionReviewInfoDto} from "../../../../interfaces/submissionReview.interface";

@Component({
  selector: 'app-contest-description',
  templateUrl: './description.component.html',
  styleUrls: ['./description.component.css']
})
export class ContestDescriptionComponent implements OnInit {
  faArrowAltCircleDown = faArrowAltCircleDown;
  faArrowAltCircleUp = faArrowAltCircleUp;
  faBoxOpen = faBoxOpen;
  faDownload = faDownload;
  faClipboardCheck = faClipboardCheck;
  faCheck = faCheck;
  faEdit = faEdit;
  faTimes = faTimes;

  public user: IUser;
  public privileged = false;
  public contestId: number;
  public contest: ContestViewDto;
  public examId: number;
  public ended: boolean;
  public reviews: SubmissionReviewInfoDto[];

  constructor(
    private title: Title,
    private route: ActivatedRoute,
    private service: ContestService,
    private auth: AuthorizeService,
    private config: ApplicationConfigService
  ) {
    this.contestId = this.route.snapshot.params.contestId;
    this.examId = this.config.examId;
  }

  ngOnInit() {
    this.auth.getUser()
      .pipe(take(1))
      .subscribe(user => {
        this.user = user;
        this.privileged = user.roles.indexOf('Administrator') >= 0
          || user.roles.indexOf('ContestManager') >= 0;
      })
    this.service.getSingle(this.contestId, true)
      .subscribe(contest => {
        this.contest = contest;
        this.ended = moment().isAfter(this.contest.endTime);
        this.title.setTitle(contest.title + ' - 题目列表');
      });
  }

  public exportReviews() {
    this.service.getReview(this.contestId)
      .subscribe(reviews => {
        this.reviews = reviews;
        const workbook = new excel.Workbook();
        const sheet = workbook.addWorksheet(this.contest.title);
        sheet.columns = ([
          {header: 'Contestant ID', key: 'id'},
          {header: 'Contestant Name', key: 'name'},
          {header: 'ProblemId', key: 'problemId'},
          {header: 'SubmissionId', key: 'submissionId'},
          {header: 'Score', key: 'score'},
          {header: 'Comment', key: 'comments'}
        ]);
        for (const review of this.reviews.slice(0)) {
          const row = {
            id: review.contestantId,
            name: review.submission.contestantId,
            problemId: review.submission.problemId,
            submissionId: review.submission.id,
            score: review.score,
            comments: review.comments
          };
          sheet.addRow(row);
        }
        workbook.xlsx.writeBuffer().then(data => {
          saveAs(new Blob([data]), this.contest.title + '-reviews.xlsx');
        });
      });
  }
}
