import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { AdminProblemService } from '../../../services/problem.service';
import { ProblemEditDto, TestCase } from '../../../../app/interfaces/problem.interfaces';
import { ActivatedRoute } from '@angular/router';
import { HttpEventType } from '@angular/common/http';

@Component({
  selector: 'app-admin-problem-test-cases',
  templateUrl: './test-cases.component.html',
  styleUrls: ['./test-cases.component.css']
})
export class AdminProblemTestCasesComponent implements OnInit {
  @ViewChild('zipFileInput') zipFileInput: ElementRef;

  public problemId: number;
  public problem: ProblemEditDto;
  public testCases: TestCase[];
  public file: File;
  public progress: number;

  constructor(
    private route: ActivatedRoute,
    private service: AdminProblemService
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
}
