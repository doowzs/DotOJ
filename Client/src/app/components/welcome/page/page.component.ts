import { Component } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { ApplicationConfigService } from '../../../services/config.service';

@Component({
  selector: 'app-welcome-page',
  templateUrl: './page.component.html',
  styleUrls: ['./page.component.css']
})
export class WelcomePageComponent {
  public examId: number;

  constructor(
    private title: Title,
    private config: ApplicationConfigService
  ) {
    this.title.setTitle(this.config.title);
    this.examId = this.config.examId;
  }
}
