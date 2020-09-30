import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpEventType } from '@angular/common/http';

import { ProblemEditDto, TestCase } from '../../../../interfaces/problem.interfaces';
import { AdminProblemService } from '../../../services/problem.service';
import { AdminSubmissionService } from '../../../services/submission.service';
import { faUpload } from '@fortawesome/free-solid-svg-icons';
import { Program } from '../../../../interfaces/submission.interfaces';

@Component({
  selector: 'app-admin-problem-tests',
  templateUrl: './tests.component.html',
  styleUrls: ['./tests.component.css']
})
export class AdminProblemTestsComponent implements OnInit {
  faUpload = faUpload;

  @ViewChild('zipFileInput') zipFileInput: ElementRef;

  public problemId: number;
  public problem: ProblemEditDto;
  public testCases: TestCase[];
  public file: File;
  public progress: number;

  public submitting = false;
  public language: number;
  public code: string;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private service: AdminProblemService,
    private submitter: AdminSubmissionService
  ) {
    this.problemId = this.route.snapshot.parent.params.problemId;
  }

  ngOnInit() {
    this.service.getSingle(this.problemId)
      .subscribe(problem => this.problem = problem);
    this.service.getTestCases(this.problemId)
      .subscribe(testCases => this.testCases = testCases);
  }

  public selectZipFile(event: any) {
    if (!event.target.files.length) {
      this.file = null;
    } else {
      this.file = event.target.files[0];
    }
  }

  public updateTestCases() {
    if (confirm(`Are you sure to upload new test cases archive?`
      + ` This will override all tests of problem #${this.problemId}.`)) {
      this.service.uploadTestCases(this.problem.id, this.file)
        .subscribe(event => {
          if (event.type === HttpEventType.UploadProgress) {
            this.progress = Math.round(100 * event.loaded / event.total);
          } else if (event.type === HttpEventType.Response) {
            this.testCases = event.body;
            this.file = this.progress = null;
            this.zipFileInput.nativeElement.value = '';
          }
        }, error => console.error(error));
    }
  }

  public submitCode(program: Program) {
    this.submitting = true;
    this.submitter.createSingle(this.problemId, program)
      .subscribe(() => {
        this.router.navigate(['/admin/submission'])
      }, error => {
        console.error(error);
        this.submitting = false;
      });
  }
}
