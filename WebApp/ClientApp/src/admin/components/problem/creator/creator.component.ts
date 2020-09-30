import { Component, ElementRef, ViewChild } from '@angular/core';
import { Router } from '@angular/router';

import { ProblemEditDto } from '../../../../interfaces/problem.interfaces';
import { AdminProblemService } from '../../../services/problem.service';
import { HttpEventType } from '@angular/common/http';
import { faUpload } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-admin-problem-creator',
  templateUrl: './creator.component.html',
  styleUrls: ['./creator.component.css']
})
export class AdminProblemCreatorComponent {
  faUpload = faUpload;

  @ViewChild('zipFileInput') zipFileInput: ElementRef;

  public contestId: number;
  public file: File;
  public progress: number;

  constructor(
    private router: Router,
    private service: AdminProblemService
  ) {
  }

  public selectZipFile(event: any) {
    if (!event.target.files.length) {
      this.file = null;
    } else {
      this.file = event.target.files[0];
    }
  }

  public importProblem() {
    this.service.importProblem(this.contestId, this.file)
      .subscribe(event => {
        if (event.type === HttpEventType.UploadProgress) {
          this.progress = Math.round(100 * event.loaded / event.total);
        } else if (event.type === HttpEventType.Response) {
          this.file = this.progress = null;
          this.zipFileInput.nativeElement.value = '';
          this.router.navigate(['/admin/problem', event.body.id]);
        }
      }, error => console.error(error));
  }

  public createProblem(problem: ProblemEditDto) {
    this.service.createSingle(problem)
      .subscribe(() => {
        this.router.navigate(['/admin/problem']);
      }, error => console.error(error));
  }
}
