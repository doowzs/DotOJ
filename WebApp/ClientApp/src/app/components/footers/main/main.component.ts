import { Component } from '@angular/core';
import { ApplicationConfigService } from '../../../services/config.service';
import * as moment from 'moment';

@Component({
  selector: 'app-footer-main',
  templateUrl: './main.component.html',
  styleUrls: ['./main.component.css']
})
export class MainFooterComponent {
  public title: string;
  public year: string;
  public author: string;
  public now: moment.Moment;

  constructor(private config: ApplicationConfigService) {
    this.title = config.title;
    this.year = new Date().getFullYear().toString();
    this.author = config.author;
    this.now = moment().add(config.diff, 'ms');
    setInterval(() => {
      this.now.add(1, 's');
    }, 1000);
  }
}
