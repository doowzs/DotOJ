import { Component } from '@angular/core';
import { Title } from '@angular/platform-browser';

@Component({
  selector: 'app-admin-root',
  templateUrl: './admin.component.html'
})
export class AdminComponent {
  constructor(
    private title: Title,
  ) {
    this.title.setTitle('Administration');
  }
}
