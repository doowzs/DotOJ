import {Component, OnInit} from '@angular/core';
import {Title} from "@angular/platform-browser";
import {FormBuilder} from "@angular/forms";
import {AuthorizeService} from "../authorize.service";
import {ActivatedRoute, Route, Router} from "@angular/router";

@Component({
  selector: 'auth-logout',
  templateUrl: './logout.component.html',
  styleUrls: ['./logout.component.css']
})
export class LogoutComponent implements OnInit {

  constructor(private router: Router,
              private authService: AuthorizeService) {
  }

  ngOnInit() {
    this.authService.removeCredentials();
    this.router.navigate(['/']);
  }
}
