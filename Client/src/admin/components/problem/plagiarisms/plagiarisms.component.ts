import { Component, OnInit } from "@angular/core";
import { ActivatedRoute } from "@angular/router";

import { PlagiarismInfoDto } from "../../../../interfaces/plagiarism.interfaces";
import { AdminProblemService } from "../../../services/problem.service";
import { ProblemEditDto } from "../../../../interfaces/problem.interfaces";
import { faRedo, faSearch, faTimes } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-admin-problem-plagiarisms',
  templateUrl: './plagiarisms.component.html',
  styleUrls: ['./plagiarisms.component.css']
})
export class AdminProblemPlagiarismsComponent implements OnInit {
  faRedo = faRedo;
  faSearch = faSearch;
  faTimes = faTimes;

  public loading = false;
  public problemId: number;
  public problem: ProblemEditDto;
  public plagiarisms: PlagiarismInfoDto[] = [];

  constructor(
    private service: AdminProblemService,
    private route: ActivatedRoute
  ) {
    this.problemId = this.route.snapshot.parent.params.problemId;
  }

  ngOnInit() {
    this.service.getSingle(this.problemId)
      .subscribe(problem => this.problem = problem);
    this.loadPlagiarisms();
  }

  loadPlagiarisms() {
    this.loading = true;
    this.service.getPlagiarisms(this.problemId)
      .subscribe(plagiarisms => {
        this.plagiarisms = plagiarisms;
        this.loading = false;
      });
  }

  checkPlagiarism() {
    if (confirm('Are you sure you want to check plagiarism for problem #' + this.problemId + '?')) {
      this.service.checkPlagiarisms(this.problemId)
        .subscribe(plagiarism => this.plagiarisms.unshift(plagiarism));
    }
  }
}
