import { Component } from '@angular/core';
import { Title } from '@angular/platform-browser';

@Component({
  selector: 'app-miscellaneous-changelog',
  templateUrl: './changelog.component.html'
})
export class ChangelogComponent {
  constructor(private title: Title) {
    this.title.setTitle('Changelog');
  }
}
