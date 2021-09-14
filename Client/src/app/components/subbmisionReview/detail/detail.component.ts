import {Component, OnInit, OnChanges} from '@angular/core';
import {ActivatedRoute, Router} from '@angular/router';
import {FormBuilder, FormArray, FormGroup, Validators} from '@angular/forms';
import {Title} from '@angular/platform-browser';
import * as moment from 'moment';

import {ContestViewDto} from '../../../../interfaces/contest.interfaces';
import {AuthorizeService, IUser} from '../../../../auth/authorize.service';
import {SubmissionReviewService} from '../../../services/submissionReview.service';
import {ContestService} from '../../../services/contest.service';
import {
  faArrowAltCircleDown,
  faArrowAltCircleUp,
  faBoxOpen,
  faCheck,
  faEdit,
  faTimes,
  faUser
} from '@fortawesome/free-solid-svg-icons';
import {take} from 'rxjs/operators';
import {SubmissionViewDto} from "../../../../interfaces/submission.interfaces";
import {SubmissionService} from "../../../services/submission.service";

@Component({
  selector: 'app-submissionReview-detail',
  templateUrl: './detail.component.html',
  styleUrls: ['./detail.component.css']
})
export class SubmissionReviewDetailComponent implements OnInit {
  faEdit = faEdit;
  reviewForm = this.formBuilder.group({
    scores: this.formBuilder.array([""]),
    comments: this.formBuilder.array([""])
  });

  public user: IUser;
  public submissions: SubmissionViewDto[];
  public contestId: number;
  public problemId: number;
  public jumpAddress: string;
  public reviewId: number;
  public reviewMap: Map<number, number>;


  constructor(
    private title: Title,
    private route: ActivatedRoute,
    private router: Router,
    private service: SubmissionReviewService,
    private formBuilder: FormBuilder
  ) {
    this.problemId = this.route.snapshot.params.problemId;
    this.contestId = this.route.parent.snapshot.params.contestId;
    this.jumpAddress = '/contest/' + this.contestId;
  }

  get scores() {
    return this.reviewForm.get('scores') as FormArray;
  }

  get comments() {
    return this.reviewForm.get('comments') as FormArray;
  }


  ngOnInit() {
    this.title.setTitle('代码互评');
    this.reviewId = 0;
    this.service.getReviewList(this.problemId)
      .subscribe(submissions => {
        this.submissions = submissions;
        this.reviewMap = new Map<number, number>();
        let count = 0;
        for (let submission of submissions) {
          count = count + 1;
          this.reviewMap.set(submission.id, count);
        }
        for (let i = 1; i < submissions.length; i = i + 1) {
          this.scores.push(this.formBuilder.control(''));
          this.comments.push(this.formBuilder.control(''));
        }
      });
  }

  GetNext() {
    this.reviewId = this.reviewId + 1;
  }

  GetPre() {
    this.reviewId = this.reviewId - 1;
  }

  GetReview(id: number) {
    this.reviewId = id;
  }

  onSubmit() {
    alert("提交成功！");
  }
}
