import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-problem-list',
  templateUrl: './list.component.html'
})
export class ProblemListComponent {
  public problems: ProblemListDto[];

  // TODO: migrate requests to problem service
  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    http.get<ProblemListPagination>(baseUrl + 'api/v1/problem').subscribe(problems => {
      this.problems = problems.items;
    }, error => console.error(error));
  }
}

interface ProblemListPagination {
  items: ProblemListDto[];
}

interface ProblemListDto {
  id: number;
  title: string;
  acceptedSubmissions: number;
  totalSubmissions: number;
}
