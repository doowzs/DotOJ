import { Component } from '@angular/core';
import { ApplicationConfigService } from '../../../services/config.service';

@Component({
  selector: 'app-footer-main',
  templateUrl: './main.component.html',
  styleUrls: ['./main.component.css']
})
export class MainFooterComponent {
  public title: string;
  public year: string;
  public author: string;

  constructor(private config: ApplicationConfigService) {
    this.title = config.title;
    this.year = new Date().getFullYear().toString();
    this.author = config.author;
  }
}
