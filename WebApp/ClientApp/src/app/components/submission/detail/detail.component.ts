import { Component, Input, OnInit } from '@angular/core';

import { SubmissionViewDto } from '../../../../interfaces/submission.interfaces';
import { SubmissionService } from '../../../services/submission.service';
import { ActivatedRoute } from '@angular/router';
import { Title } from '@angular/platform-browser';

@Component({
  selector: 'app-submission-detail',
  templateUrl: './detail.component.html',
  styleUrls: ['./detail.component.css']
})
export class SubmissionDetailComponent implements OnInit {

  public submissionId: number;
  public submission: SubmissionViewDto;

  constructor(
    private title: Title,
    private route: ActivatedRoute,
    private service: SubmissionService
  ) {
    this.submissionId = this.route.snapshot.params.submissionId;
    this.title.setTitle('Submission #' + this.submissionId);
  }

  ngOnInit() {
    this.service.getSingleAsView(this.submissionId)
      .subscribe(submission => this.submission = submission);
  }
}
