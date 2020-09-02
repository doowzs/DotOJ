import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import { AdminSubmissionService } from '../../../services/submission.service';
import { SubmissionEditDto } from '../../../../app/interfaces/submission.interfaces';

@Component({
  selector: 'app-admin-submission-editor',
  templateUrl: './editor.component.html',
  styleUrls: ['./editor.component.css']
})
export class AdminSubmissionEditorComponent implements OnInit {
  public edit: boolean;
  public submissionId: number;
  public submission: SubmissionEditDto;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private service: AdminSubmissionService
  ) {
    this.edit = this.route.snapshot.queryParams.edit ?? false;
    this.submissionId = this.route.snapshot.params.submissionId;
  }

  ngOnInit() {
    this.service.getSingle(this.submissionId)
      .subscribe(submission => this.submission = submission);
  }

  public editSubmission() {
    this.edit = true;
    this.router.navigate(['/admin/submission', this.submissionId], {
      queryParams: { edit: true }
    });
  }

  public updateSubmission(submission: SubmissionEditDto) {
    this.service.updateSingle(submission).subscribe(updated => {
      this.edit = false;
      this.submission = updated;
      this.router.navigate(['/admin/submission', this.submissionId]);
    }, error => console.error(error));
  }

  public deleteSubmission() {
    this.service.deleteSingle(this.submissionId).subscribe(() => {
      this.router.navigate(['/admin/submission']);
    }, error => console.error(error));
  }
}
