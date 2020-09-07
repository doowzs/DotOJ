import { Component, ViewChild } from '@angular/core';
import { AdminProblemService } from '../../../services/problem.service';
import { saveAs } from 'file-saver';

@Component({
  selector: 'app-admin-problem-archive',
  templateUrl: './archive.component.html',
  styleUrls: ['./archive.component.css']
})
export class AdminProblemArvhiceComponent {
  public exportProblemId: number;

  constructor(private service: AdminProblemService) {
  }

  public exportProblem(event: any) {
    this.service.exportProblem(this.exportProblemId)
      .subscribe(blob => { saveAs(blob, this.exportProblemId + '.zip'); });
  }
}
