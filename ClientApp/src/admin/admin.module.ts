﻿import {NgModule} from '@angular/core';
import {CommonModule} from '@angular/common';
import {RouterModule} from '@angular/router';

import {AuthorizeGuard} from '../api-authorization/authorize.guard';
import {ApiAuthorizationModule} from '../api-authorization/api-authorization.module';

import {AdminComponent} from './admin.component';
import {DashboardComponent} from './dashboard/dashboard.component';
import {MatToolbarModule} from '@angular/material/toolbar';
import {MatButtonModule} from '@angular/material/button';
import {MatIconModule} from '@angular/material/icon';
import {MatSidenavModule} from '@angular/material/sidenav';
import {MatListModule} from "@angular/material/list";

@NgModule({
  imports: [
    CommonModule,
    RouterModule.forChild([
      {
        path: 'admin',
        component: AdminComponent,
        canActivate: [AuthorizeGuard],
        children: [
          {
            path: '',
            component: DashboardComponent,
            pathMatch: 'full'
          }
        ]
      }
    ]),
    ApiAuthorizationModule,
    MatToolbarModule,
    MatButtonModule,
    MatIconModule,
    MatSidenavModule,
    MatListModule,
  ],
  declarations: [
    AdminComponent,
    DashboardComponent
  ],
  exports: [
    RouterModule
  ]
})
export class AdminModule {
}
