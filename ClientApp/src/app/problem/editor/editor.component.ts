import {Component} from '@angular/core';
import {ActivatedRoute, Router} from '@angular/router';
import {ProblemService} from '../problem.service';

@Component({
  selector: 'app-problem-code-editor',
  templateUrl: './editor.component.html'
})
export class ProblemCodeEditorComponent {
  public options = {theme: 'vs-dark'};
  public code: string;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private service: ProblemService
  ) {
  }
}
