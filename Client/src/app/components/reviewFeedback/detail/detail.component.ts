import {Component, OnInit} from '@angular/core';
import {ActivatedRoute, Router} from '@angular/router';
import {Title} from '@angular/platform-browser';

import {IUser} from '../../../../auth/authorize.service';
import {ReviewFeedbackDService} from '../../../services/reviewFeedback.service';
import {SubmissionReviewInfoDto} from '../../../../interfaces/submissionReview.interface';

@Component({
  selector: 'app-submissionReview-detail',
  templateUrl: './detail.component.html',
  styleUrls: ['./detail.component.css']
})
export class ReviewFeedbackDetailComponent implements OnInit {
  public user: IUser;
  public feedbacks: SubmissionReviewInfoDto[];
  public contestId: number;
  public problemId: number;
  public errorMessage: string;
  public reviewId: number;
  public reviewMap: Map<number, number>;


  constructor(
    private title: Title,
    private route: ActivatedRoute,
    private router: Router,
    private service: ReviewFeedbackDService,
  ) {
    this.errorMessage = null;
    this.problemId = this.route.snapshot.params.problemId;
    this.contestId = this.route.parent.snapshot.params.contestId;
  }

  ngOnInit() {
    this.title.setTitle('互评反馈');
    this.reviewId = 0;
    this.service.getFeedbackList(this.problemId)
      .subscribe(feedbacks => {
        this.feedbacks = feedbacks;
      }, (err) => {
        alert(err.error);
        this.errorMessage = err.error;
      });
  }
}
