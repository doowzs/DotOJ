import { Component } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { faCheck, faDownload, faList } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-admin-problem-export',
  templateUrl: './export.component.html',
  styleUrls: ['./export.component.css']
})
export class AdminProblemExportComponent {
  faCheck = faCheck;
  faDownload = faDownload;
  faList = faList;

  public problemId: number;

  constructor(private route: ActivatedRoute) {
    this.problemId = this.route.snapshot.parent.params.problemId;
  }

  public exportProblem() {
    window.open('/api/v1/admin/problem/' + this.problemId + '/export', '_blank');
  }

  public exportProblemSubmissions(all: boolean = false) {
    window.open('/api/v1/admin/problem/' + this.problemId + '/export/submissions?all=' + all, '_blank');
  }
}
