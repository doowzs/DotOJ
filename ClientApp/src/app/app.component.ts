import { Component } from '@angular/core';
import { ApplicationConfigService } from 'src/app/app.config.service';
import { Title } from '@angular/platform-browser';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html'
})
export class AppComponent {
  constructor(
    private title: Title,
    private config: ApplicationConfigService
  ) {
    this.title.setTitle(config.title);
  }
}
