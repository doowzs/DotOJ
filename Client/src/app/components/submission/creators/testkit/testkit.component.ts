import { Component, Input, OnInit, OnChanges, SimpleChanges, ViewChild, ElementRef } from '@angular/core';
import { HttpEventType } from "@angular/common/http";

import { SubmissionService } from '../../../../services/submission.service';
import { ProblemViewDto } from "../../../../../interfaces/problem.interfaces";
import { ProblemService } from "../../../../services/problem.service";
import { faSyncAlt, faTimes, faUpload } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-submission-testkit-creator',
  templateUrl: './testkit.component.html',
  styleUrls: ['./testkit.component.css']
})
export class SubmissionTestKitCreatorComponent implements OnInit, OnChanges {
  faSyncAlt = faSyncAlt;
  faTimes = faTimes;
  faUpload = faUpload;

  @Input() public problemId: number;

  @ViewChild('zipFileInput') zipFileInput: ElementRef;

  public problem: ProblemViewDto;
  public api: string;
  public token: string;
  public progress: number;
  public result: string;
  public error: string;

  constructor(
    private problemService: ProblemService,
    private submissionService: SubmissionService
  ) {
    this.api = window.location.protocol + '//' + window.location.host + '/api/v1/submission/lab';
  }

  ngOnInit() {
    this.loadProblemAndToken();
  }

  ngOnChanges(changes: SimpleChanges) {
    if (!!changes.problemId) {
      this.problem = null;
      this.token = null;
      this.loadProblemAndToken();
    }
  }

  public loadProblemAndToken() {
    this.problemService.getSingle(this.problemId)
      .subscribe(problem => {
        this.problem = problem;
        if (problem.type === 1) {
          this.loadLabSubmitToken();
        }
      });
  }

  public loadLabSubmitToken() {
    this.submissionService.getLabSubmitToken(this.problemId)
      .subscribe(token => this.token = token);
  }

  public createLabSubmission(event: any) {
    if (event.target.files.length) {
      const file = event.target.files[0];
      if (file.size > 10 * 1024 * 1024) {
        this.error = "file is larger than 10MiB.";
      } else {
        this.submissionService.createLabSubmission(this.token, file)
          .subscribe(event => {
            if (event.type === HttpEventType.UploadProgress) {
              this.progress = Math.round(100 * event.loaded / event.total);
            } else if (event.type === HttpEventType.Response) {
              this.result = event.body;
              this.progress = null;
              this.zipFileInput.nativeElement.value = '';
              this.loadLabSubmitToken();
              this.submissionService.getSingleAsInfo(parseInt(this.result.match(/#(\d+)/)[1]))
                .subscribe(submission => this.submissionService.newSubmission.next(submission));
            }
          }, error => {
            console.log(error);
            this.error = error.error;
            this.loadLabSubmitToken();
          });
      }
    }
  }
}
