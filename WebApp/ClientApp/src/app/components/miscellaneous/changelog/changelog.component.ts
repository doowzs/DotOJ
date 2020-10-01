import { Component } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { ApplicationConfigService } from '../../../services/config.service';

@Component({
  selector: 'app-miscellaneous-changelog',
  templateUrl: './changelog.component.html'
})
export class ChangelogComponent {
  constructor(
    private title: Title,
    public config: ApplicationConfigService
  ) {
    this.title.setTitle('Changelog');
  }
}
