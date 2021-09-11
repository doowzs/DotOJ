import { Component, OnInit , Input} from '@angular/core';
import { ActivatedRoute , Router} from '@angular/router';
import { Title } from '@angular/platform-browser';
import * as moment from 'moment';

import { ContestViewDto } from '../../../../interfaces/contest.interfaces';
import { AuthorizeService, IUser } from '../../../../auth/authorize.service';
import { SubmissionReviewService } from '../../../services/submissionReview.service';
import { ContestService } from '../../../services/contest.service';
import {
  faArrowAltCircleDown,
  faArrowAltCircleUp,
  faBoxOpen,
  faCheck,
  faEdit,
  faTimes,
  faUser
} from '@fortawesome/free-solid-svg-icons';
import { take } from 'rxjs/operators';
import {SubmissionViewDto} from "../../../../interfaces/submission.interfaces";
import {SubmissionService} from "../../../services/submission.service";

@Component({
  selector: 'app-submissionReview-detail',
  templateUrl: './detail.component.html',
  styleUrls: ['./detail.component.css']
})
export class SubmissionReviewDetailComponent implements OnInit {
  public user: IUser;
  public submissions : SubmissionViewDto[];
  public contestId: number;
  public problemId: number;
  public jumpAddress: string;
  constructor(
    private title: Title,
    private route: ActivatedRoute,
    private router: Router,
    private service: SubmissionReviewService,
  ) {
    this.problemId = this.route.snapshot.params.problemId;
    this.contestId = this.route.parent.snapshot.params.contestId;
    this.jumpAddress = '/contest/' + this.contestId;
  }

  ngOnInit() {
    this.service.getReviewList(this.problemId)
      .subscribe(submissions => {
        this.submissions = submissions;
      },
        (error) => {
          alert(error.error);
          this.router.navigate([this.jumpAddress]);
        }
      );
  }
}
