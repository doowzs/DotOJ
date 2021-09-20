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
  faSyncAlt,
  faTimes,
  faUser
} from '@fortawesome/free-solid-svg-icons';
import {take} from 'rxjs/operators';
import {Program, SubmissionViewDto} from "../../../../interfaces/submission.interfaces";
import {SubmissionService} from "../../../services/submission.service";

@Component({
  selector: 'app-submissionReview-detail',
  templateUrl: './detail.component.html',
  styleUrls: ['./detail.component.css']
})
export class SubmissionReviewDetailComponent implements OnInit {
  faEdit = faEdit;
  faSyncAlt = faSyncAlt;
  reviewForm = this.formBuilder.group({
    scores: this.formBuilder.array(
      [this.formBuilder.control("", [Validators.required, Validators.min(0), Validators.max(10)])]
    ),
    comments: this.formBuilder.array(
      [this.formBuilder.control("", [Validators.required, Validators.minLength(5), Validators.maxLength(10000)])]
    )
  });

  public user: IUser;
  public submissions: SubmissionViewDto[];
  public contestId: number;
  public problemId: number;
  public errorMessage: string;
  public reviewId: number;
  public reviewMap: Map<number, number>;
  public score: number[];
  public comment: string[];
  public submissionId: number[];
  public jumpAddress: string;
  public submitting = false;

  constructor(
    private title: Title,
    private route: ActivatedRoute,
    private router: Router,
    private service: SubmissionReviewService,
    private createService: SubmissionService,
    private formBuilder: FormBuilder
  ) {
    this.errorMessage = null;
    this.problemId = this.route.snapshot.params.problemId;
    this.contestId = this.route.parent.snapshot.params.contestId;
  }

  get scores() {
    return this.reviewForm.get('scores') as FormArray;
  }

  get comments() {
    return this.reviewForm.get('comments') as FormArray;
  }


  ngOnInit() {
    this.title.setTitle('代码互评');
    this.jumpAddress = '/contest/' + this.contestId;
    this.reviewId = 0;
    this.score = [];
    this.comment = [];
    this.submissionId = [];
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
          this.scores.push(this.formBuilder.control("", [Validators.required, Validators.min(0), Validators.max(10)]));
          this.comments.push(this.formBuilder.control("", [Validators.required, Validators.minLength(5), Validators.maxLength(10000)]));
        }
      },(err) => {
        this.errorMessage = err.error;
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
    if (this.submissions != null) {
      for (let i = 0; i < this.submissions.length; i = i + 1) {
        this.comment.push(this.comments.controls[i].value);
        this.score.push(this.scores.controls[i].value);
        this.submissionId.push(this.submissions[i].id);
      }
      this.service.createReview(this.submissionId, this.problemId, this.score, this.comment)
          .subscribe(message => {
              alert(message);
              this.router.navigate([this.jumpAddress]);
          });
    }
  }

  public submit(program: Program): void {
    this.submitting = true;
    this.createService.createSingle(this.problemId, program)
      .subscribe(submission => {
        setTimeout(() => {
          this.submitting = false;
        }, 5000);
      }, error => {
        console.log(error);
        this.submitting = false;
      });
  }

}
