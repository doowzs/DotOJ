import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

import { AuthorizeGuard } from '../api-authorization/authorize.guard';
import { ApiAuthorizationModule } from '../api-authorization/api-authorization.module';

import { AdminComponent } from './admin.component';
import { AdminDashboardComponent } from './components/dashboard/dashboard.component';

@NgModule({
  imports: [
    CommonModule,
    RouterModule.forChild([
      {
        path: 'admin', component: AdminComponent, canActivate: [AuthorizeGuard],
        children: [
          { path: '', pathMatch: 'full', component: AdminDashboardComponent }
        ]
      }
    ]),
    ApiAuthorizationModule
  ],
  declarations: [
    AdminComponent,
    AdminDashboardComponent
  ],
  exports: [
    RouterModule
  ]
})
export class AdminModule {
}
