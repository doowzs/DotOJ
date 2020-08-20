﻿import {NgModule} from '@angular/core';
import {CommonModule} from '@angular/common';
import {RouterModule} from '@angular/router';
import {FormsModule, ReactiveFormsModule} from '@angular/forms';

import {AuthorizeGuard} from '../api-authorization/authorize.guard';
import {ApiAuthorizationModule} from '../api-authorization/api-authorization.module';

import {AdminComponent} from './admin.component';
import {AdminDashboardComponent} from './dashboard/dashboard.component';
import {AdminAssignmentFormComponent} from './assignment/form/form.component';
import {AdminAssignmentListComponent} from './assignment/list/list.component';
import {AdminAssignmentCreateComponent} from './assignment/create/create.component';

import {MatToolbarModule} from '@angular/material/toolbar';
import {MatButtonModule} from '@angular/material/button';
import {MatIconModule} from '@angular/material/icon';
import {MatSidenavModule} from '@angular/material/sidenav';
import {MatListModule} from '@angular/material/list';
import {MatFormFieldModule} from '@angular/material/form-field';
import {MatInputModule} from '@angular/material/input';
import {MatSelectModule} from '@angular/material/select';
import {MatTableModule} from '@angular/material/table';
import {MatPaginatorModule} from '@angular/material/paginator';

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
            component: AdminDashboardComponent,
            pathMatch: 'full'
          },
          {
            path: 'assignment',
            component: AdminAssignmentListComponent,
            pathMatch: 'full',
          },
          {
            path: 'assignment/create',
            component: AdminAssignmentCreateComponent
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
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    FormsModule,
    ReactiveFormsModule,
    MatTableModule,
    MatPaginatorModule,
  ],
  declarations: [
    AdminComponent,
    AdminDashboardComponent,
    AdminAssignmentFormComponent,
    AdminAssignmentListComponent,
    AdminAssignmentCreateComponent,
  ],
  exports: [
    RouterModule
  ]
})
export class AdminModule {
}
