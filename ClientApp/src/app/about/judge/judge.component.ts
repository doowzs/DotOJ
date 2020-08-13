import {Component} from '@angular/core';

import {Languages} from '../../app.interfaces';

@Component({
  selector: 'app-about-judge-info',
  templateUrl: './judge.component.html'
})
export class JudgeInfoComponent {
  public languages = Languages;
  public columns = ['name', 'env', 'option'];
}
