import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpEventType } from '@angular/common/http';

import { Languages } from '../../../../consts/languages.consts';
import { ProblemEditDto, TestCase } from '../../../../interfaces/problem.interfaces';
import { AdminProblemService } from '../../../services/problem.service';
import { AdminSubmissionService } from '../../../services/submission.service';

@Component({
  selector: 'app-admin-problem-test-cases',
  templateUrl: './test-cases.component.html',
  styleUrls: ['./test-cases.component.css']
})
export class AdminProblemTestCasesComponent implements OnInit {
  Languages = Languages;
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

  public submitCode() {
    this.submitting = true;
    this.submitter.createSingle(this.problem.id, this.language, this.code)
      .subscribe(() => {
        this.router.navigate(['/admin/submission'])
      }, error => {
        console.error(error);
        this.submitting = false;
      });
  }
}
