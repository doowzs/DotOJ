import { Component } from '@angular/core';
import { AdminSubmissionService } from '../../../services/submission.service';
import { FormBuilder, FormGroup } from '@angular/forms';
import { Router } from '@angular/router';
import { faRedo } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-admin-submission-rejudge',
  templateUrl: './rejudge.component.html',
  styleUrls: ['./rejudge.component.css']
})
export class AdminSubmissionRejudgeComponent {
  faRedo = faRedo;

  public form: FormGroup;

  constructor(
    private router: Router,
    private builder: FormBuilder,
    private service: AdminSubmissionService
  ) {
    this.form = this.builder.group({
      contestId: [null, []],
      problemId: [null, []],
      submissionId: [null, []],
    }, {
      validators: [this.validateAtLeastOneField]
    });
  }

  public validateAtLeastOneField(form: FormGroup): any {
    if (form.get('contestId').value || form.get('problemId').value || form.get('submissionId').value) {
      return null;
    }
    return { atLeastOneField: true };
  }

  public submitForm(data: any) {
    if (confirm(`Are you sure to rejudge all specified submissions where contest=${data.contestId}`
      + ` problem=${data.problemId} submission=${data.submissionId}?`)) {
      this.service.rejudge(data.contestId, data.problemId, data.submissionId)
        .subscribe(() => {
          this.router.navigate(['/admin/submission']);
        });
    }
  }
}
