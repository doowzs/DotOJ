import { Component } from '@angular/core';
import { Title } from '@angular/platform-browser';

import { ApplicationConfigService } from './services/config.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html'
})
export class AppComponent {
  constructor(
    private title: Title,
    private config: ApplicationConfigService
  ) {
    this.title.setTitle(this.config.title);
  }
}
