import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { ReactiveFormsModule } from '@angular/forms';

import { AuthorizeGuard } from '../api-authorization/authorize.guard';
import { ApiAuthorizationModule } from '../api-authorization/api-authorization.module';

import { AdminComponent } from './admin.component';
import { AdminDashboardComponent } from './components/dashboard/dashboard.component';
import { NzLayoutModule } from 'ng-zorro-antd/layout';
import { NzMenuModule } from 'ng-zorro-antd/menu';
import { NzCardModule } from 'ng-zorro-antd/card';
import { NzPageHeaderModule } from 'ng-zorro-antd/page-header';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { AdminContestListComponent } from './components/contest/list/list.component';
import { AdminContestCreatorComponent } from './components/contest/creator/creator.component';
import { AdminContestEditorComponent } from './components/contest/editor/editor.component';
import { AdminContestFormComponent } from './components/contest/form/form.component';
import { NzFormModule } from 'ng-zorro-antd/form';
import { NzInputModule } from 'ng-zorro-antd/input';
import { NzSelectModule } from 'ng-zorro-antd/select';
import { NzDatePickerModule } from 'ng-zorro-antd/date-picker';
import { NzCheckboxModule } from 'ng-zorro-antd/checkbox';

@NgModule({
  imports: [
    CommonModule,
    BrowserModule,
    BrowserAnimationsModule,
    ReactiveFormsModule,
    RouterModule.forChild([
      {
        path: 'admin', component: AdminComponent, canActivate: [AuthorizeGuard],
        children: [
          { path: '', pathMatch: 'full', component: AdminDashboardComponent },
          {
            path: 'contest', children: [
              { path: '', pathMatch: 'full', component: AdminContestListComponent },
              { path: 'new', component: AdminContestCreatorComponent },
              { path: ':id', component: AdminContestEditorComponent }
            ]
          }
        ]
      }
    ]),
    ApiAuthorizationModule,
    NzLayoutModule,
    NzMenuModule,
    NzCardModule,
    NzPageHeaderModule,
    NzButtonModule,
    NzIconModule,
    NzFormModule,
    NzInputModule,
    NzSelectModule,
    NzDatePickerModule,
    NzCheckboxModule
  ],
  declarations: [
    AdminComponent,
    AdminDashboardComponent,
    AdminContestListComponent,
    AdminContestFormComponent,
    AdminContestCreatorComponent,
    AdminContestEditorComponent
  ],
  exports: [
    RouterModule
  ]
})
export class AdminModule {
}
