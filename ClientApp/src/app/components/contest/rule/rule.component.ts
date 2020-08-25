import { Component, Input } from '@angular/core';
import { ContestMode } from '../../../interfaces/contest.interfaces';

@Component({
  selector: 'app-contest-rule',
  templateUrl: './rule.component.html',
  styleUrls: ['./rule.component.css']
})
export class ContestRuleComponent {
  ContestMode = ContestMode;
  @Input() public mode: ContestMode;

  constructor() {
  }
}
