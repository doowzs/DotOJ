import { Component, Input, Optional, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Title } from '@angular/platform-browser';

import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { SubmissionViewDto } from '../../../../interfaces/submission.interfaces';
import { SubmissionService } from '../../../services/submission.service';

@Component({
  selector: 'app-submission-detail',
  templateUrl: './detail.component.html',
  styleUrls: ['./detail.component.css']
})
export class SubmissionDetailComponent implements OnInit {

  @Input() submissionId: number;
  @Input() standalone: boolean = true;
  public submission: SubmissionViewDto;
  public unauthorized: boolean = false;

  constructor(
    private title: Title,
    private route: ActivatedRoute,
    private service: SubmissionService,
    @Optional() public activeModal: NgbActiveModal
  ) {
  }

  ngOnInit() {
    if (this.standalone) {
      this.submissionId = this.route.snapshot.params.submissionId;
      this.title.setTitle('Submission #' + this.submissionId);
    }
    this.service.getSingleAsView(this.submissionId)
      .subscribe(submission => {
        this.submission = submission;
      }, error => {
        if (error.status === 401) {
          this.unauthorized = true;
        }
      });
  }
}
