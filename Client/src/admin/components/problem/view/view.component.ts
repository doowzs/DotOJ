import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

import { ProblemEditDto } from "../../../../interfaces/problem.interfaces";
import { AdminProblemService } from "../../../services/problem.service";
import { faArchive, faBoxes, faEdit, faSearch } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-admin-problem-view',
  templateUrl: './view.component.html',
  styleUrls: ['./view.component.css']
})
export class AdminProblemViewComponent implements OnInit {
  faArchive = faArchive;
  faBoxes = faBoxes;
  faEdit = faEdit;
  faSearch = faSearch;

  public problemId: number;
  public problem: ProblemEditDto;

  constructor(
    private service: AdminProblemService,
    private route: ActivatedRoute
  ) {
    this.problemId = this.route.snapshot.params.problemId;
  }

  ngOnInit() {
    this.service.getSingle(this.problemId)
      .subscribe(problem => this.problem = problem);
  }
}
