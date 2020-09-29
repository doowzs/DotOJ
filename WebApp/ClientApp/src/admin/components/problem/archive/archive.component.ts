import { Component, ElementRef, ViewChild } from '@angular/core';
import { HttpEventType } from '@angular/common/http';
import { Router } from '@angular/router';

import { AdminProblemService } from '../../../services/problem.service';

@Component({
  selector: 'app-admin-problem-archive',
  templateUrl: './archive.component.html',
  styleUrls: ['./archive.component.css']
})
export class AdminProblemArchiveComponent {
  @ViewChild('zipFileInput') zipFileInput: ElementRef;

  public importContestId: number;
  public exportProblemId: number;
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

  public importProblem(event: any) {
    this.service.importProblem(this.importContestId, this.file)
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

  public exportProblem(event: any) {
    window.open('/api/v1/admin/problem/' + this.exportProblemId + '/export', '_blank');
  }
}
