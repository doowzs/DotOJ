import { Component } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { Verdicts } from '../../../../consts/verdicts.consts';

@Component({
  selector: 'app-miscellaneous-help',
  templateUrl: './help.component.html',
  styleUrls: ['./help.component.css']
})
export class HelpComponent {
  Vercicts = Verdicts;

  constructor(private title: Title) {
    this.title.setTitle('Help');
  }
}
