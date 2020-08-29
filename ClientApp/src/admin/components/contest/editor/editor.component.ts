import { Component } from '@angular/core';

import { AdminContestService } from '../../../services/contest.service';

@Component({
  selector: 'app-admin-contest-editor',
  templateUrl: './editor.component.html',
  styleUrls: ['./editor.component.css']
})
export class AdminContestEditorComponent {
  constructor(private service: AdminContestService) {
  }
}
