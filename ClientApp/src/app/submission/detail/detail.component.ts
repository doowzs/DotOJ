import {Component, Inject} from '@angular/core';
import {MAT_DIALOG_DATA} from "@angular/material/dialog";
import {SubmissionViewDto} from "../../app.interfaces";

@Component({
  selector: 'app-submission-detail',
  templateUrl: './detail.component.html'
})
export class SubmissionDetailComponent {
  public submission: SubmissionViewDto;

  constructor(@Inject(MAT_DIALOG_DATA) public submissionId: number) {
  }
}
