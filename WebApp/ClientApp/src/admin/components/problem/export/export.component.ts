import { Component } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { faDownload } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-admin-problem-export',
  templateUrl: './export.component.html',
  styleUrls: ['./export.component.css']
})
export class AdminProblemExportComponent {
  faDownload = faDownload;

  public problemId: number;

  constructor(private route: ActivatedRoute) {
    this.problemId = this.route.snapshot.parent.params.problemId;
  }

  public exportProblem() {
    window.open('/api/v1/admin/problem/' + this.problemId + '/export', '_blank');
  }
}
