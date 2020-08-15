import {NgModule} from '@angular/core';
import {RouterModule} from '@angular/router';

import {AuthorizeGuard} from '../api-authorization/authorize.guard';

import {AdminComponent} from './admin.component';
import {DashboardComponent} from './dashboard/dashboard.component';

@NgModule({
  imports: [
    RouterModule.forChild([
      {
        path: 'admin',
        component: AdminComponent,
        children: [
          {
            path: '',
            component: DashboardComponent,
            pathMatch: 'full'
          }
        ]
      }
    ]),
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
