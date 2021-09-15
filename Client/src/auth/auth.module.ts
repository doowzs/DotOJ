import {NgModule} from '@angular/core';
import {CommonModule} from '@angular/common';
import {RouterModule} from '@angular/router';
import {FormsModule, ReactiveFormsModule} from "@angular/forms";
import {HttpClientModule} from '@angular/common/http';
import {FontAwesomeModule} from '@fortawesome/angular-fontawesome';

import {LoginComponent} from './login/login.component';
import {LogoutComponent} from "./logout/logout.component";
import {ProfileComponent} from './profile/profile.component';
@NgModule({
  imports: [
    CommonModule,
    HttpClientModule,
    RouterModule.forChild(
      [
        {path: 'auth/login', component: LoginComponent},
        {path: 'auth/logout', component: LogoutComponent},
        {path: 'auth/profile', component: ProfileComponent}
      ]
    ),
    FontAwesomeModule,
    FormsModule,
    ReactiveFormsModule
  ],
  declarations:
    [LoginComponent,
    ProfileComponent
    ],
  exports:
    [
      LoginComponent,
      ProfileComponent,
    ]
})
export class AuthModule {
}
