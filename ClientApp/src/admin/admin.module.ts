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
import { AdminContestListComponent } from './components/contest/list/list.component';
import { AdminContestCreatorComponent } from './components/contest/creator/creator.component';
import { AdminContestEditorComponent } from './components/contest/editor/editor.component';
import { AdminContestFormComponent } from './components/contest/form/form.component';
import { AdminProblemListComponent } from './components/problem/list/list.component';
import { AdminProblemCreatorComponent } from './components/problem/creator/creator.component';
import { AdminProblemEditorComponent } from './components/problem/editor/editor.component';
import { AdminProblemFormComponent } from './components/problem/form/form.component';
import { AdminProblemTestCasesComponent } from './components/problem/test-cases/test-cases.component';

import { NzLayoutModule } from 'ng-zorro-antd/layout';
import { NzMenuModule } from 'ng-zorro-antd/menu';
import { NzCardModule } from 'ng-zorro-antd/card';
import { NzPageHeaderModule } from 'ng-zorro-antd/page-header';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { NzFormModule } from 'ng-zorro-antd/form';
import { NzInputModule } from 'ng-zorro-antd/input';
import { NzSelectModule } from 'ng-zorro-antd/select';
import { NzDatePickerModule } from 'ng-zorro-antd/date-picker';
import { NzCheckboxModule } from 'ng-zorro-antd/checkbox';
import { NzTableModule } from 'ng-zorro-antd/table';
import { NzPopconfirmModule } from 'ng-zorro-antd/popconfirm';
import { NzDividerModule } from 'ng-zorro-antd/divider';

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
                            { path: ':contestId', component: AdminContestEditorComponent }
                        ]
                    },
                    {
                        path: 'problem', children: [
                            { path: '', pathMatch: 'full', component: AdminProblemListComponent },
                            { path: 'new', component: AdminProblemCreatorComponent },
                            {
                                path: ':problemId', children: [
                                    { path: '', pathMatch: 'full', component: AdminProblemEditorComponent },
                                    { path: 'test-cases', component: AdminProblemTestCasesComponent }
                                ]
                            }
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
        NzCheckboxModule,
        NzTableModule,
        NzPopconfirmModule,
        NzDividerModule,
    ],
  declarations: [
    AdminComponent,
    AdminDashboardComponent,
    AdminContestListComponent,
    AdminContestFormComponent,
    AdminContestCreatorComponent,
    AdminContestEditorComponent,
    AdminProblemListComponent,
    AdminProblemFormComponent,
    AdminProblemCreatorComponent,
    AdminProblemEditorComponent,
    AdminProblemTestCasesComponent
  ],
  exports: [
    RouterModule
  ]
})
export class AdminModule {
}
