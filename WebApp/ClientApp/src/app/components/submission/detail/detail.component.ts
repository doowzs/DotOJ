import { Component, Input, OnInit } from '@angular/core';
import { SubmissionService } from '../../../services/submission.service';
import { SubmissionViewDto } from '../../../interfaces/submission.interfaces';

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
