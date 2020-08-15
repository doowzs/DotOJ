import {Component} from '@angular/core';
import {Router, RoutesRecognized} from '@angular/router';
import {filter, map} from 'rxjs/operators';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html'
})
export class AppComponent {
  public showNavbar = false;

  constructor(private router: Router) {
    this.router.events.pipe(filter(e => e instanceof RoutesRecognized))
      .pipe(map((e: RoutesRecognized) => e.state.root.firstChild.data.navbar))
      .subscribe((nb: boolean) => this.showNavbar = nb);
  }
}
