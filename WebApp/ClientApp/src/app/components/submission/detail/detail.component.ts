import { Component, Input, OnInit } from '@angular/core';

import { SubmissionViewDto } from '../../../../interfaces/submission.interfaces';
import { SubmissionService } from '../../../services/submission.service';

@Component({
  selector: 'app-submission-detail',
  templateUrl: './detail.component.html',
  styleUrls: ['./detail.component.css']
})
export class SubmissionDetailComponent implements OnInit {

  @Input() public submissionId: number;
  public submission: SubmissionViewDto;

  constructor(
    private service: SubmissionService
  ) {
  }

  ngOnInit() {
    this.service.getSingleAsView(this.submissionId)
      .subscribe(submission => this.submission = submission);
  }
}
