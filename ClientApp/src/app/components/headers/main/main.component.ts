import { Component } from '@angular/core';
import { ApplicationConfigService } from '../../../services/config.service';

@Component({
  selector: 'app-header-main',
  templateUrl: './main.component.html',
  styleUrls: ['./main.component.css']
})
export class MainHeaderComponent {
  public title: string;

  constructor(private config: ApplicationConfigService) {
    this.title = config.title;
  }
}
