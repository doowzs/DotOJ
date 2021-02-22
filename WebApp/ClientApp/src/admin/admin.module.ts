import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { MarkdownModule } from '../lib/markdown/markdown.module';
import { DatetimeModule } from '../lib/datetime/datetime.module';
import { EditorModule } from '../lib/editor/editor.module';
import { VditorModule } from '../lib/vditor/vditor.module';

import { AuthorizeGuard } from '../api-authorization/authorize.guard';
import { ApiAuthorizationModule } from '../api-authorization/api-authorization.module';

import { AdminGuard } from './admin.guard';
import { AdminComponent } from './admin.component';
import { AdminDashboardComponent } from './components/dashboard/dashboard.component';
import { AdminBulletinListComponent } from './components/bulletin/list/list.component';
import { AdminBulletinFormComponent } from './components/bulletin/form/form.component';
import { AdminBulletinCreatorComponent } from './components/bulletin/creator/creator.component';
import { AdminBulletinEditorComponent } from './components/bulletin/editor/editor.component';
import { AdminUserListComponent } from './components/user/list/list.component';
import { AdminUserFormComponent } from './components/user/form/form.component';
import { AdminUserEditorComponent } from './components/user/editor/editor.component';
import { AdminContestListComponent } from './components/contest/list/list.component';
import { AdminContestViewComponent } from './components/contest/view/view.component';
import { AdminContestFormComponent } from './components/contest/form/form.component';
import { AdminContestCreatorComponent } from './components/contest/creator/creator.component';
import { AdminContestEditorComponent } from './components/contest/editor/editor.component';
import { AdminContestRegistrationsComponent } from './components/contest/registrations/registrations.component';
import { AdminProblemListComponent } from './components/problem/list/list.component';
import { AdminProblemViewComponent } from './components/problem/view/view.component';
import { AdminProblemFormComponent } from './components/problem/form/form.component';
import { AdminProblemCreatorComponent } from './components/problem/creator/creator.component';
import { AdminProblemEditorComponent } from './components/problem/editor/editor.component';
import { AdminProblemTestsComponent } from './components/problem/tests/tests.component';
import { AdminProblemExportComponent } from './components/problem/export/export.component';
import { AdminProblemPlagiarismsComponent } from './components/problem/plagiarisms/plagiarisms.component';
import { AdminSubmissionListComponent } from './components/submission/list/list.component';
import { AdminSubmissionFormComponent } from './components/submission/form/form.component';
import { AdminSubmissionEditorComponent } from './components/submission/editor/editor.component';
import { AdminSubmissionRejudgeComponent } from './components/submission/rejudge/rejudge.component';
import { VerdictModule } from '../lib/verdict/verdict.module';

@NgModule({
  imports: [
    CommonModule,
    BrowserModule,
    BrowserAnimationsModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule.forChild([
      {
        path: 'admin',
        component: AdminComponent,
        canActivate: [AuthorizeGuard, AdminGuard],
        data: { roles: ['*'] },
        children: [
          { path: '', pathMatch: 'full', component: AdminDashboardComponent },
          {
            path: 'bulletin',
            canActivate: [AdminGuard],
            data: { roles: ['Administrator'] },
            children: [
              { path: '', pathMatch: 'full', component: AdminBulletinListComponent },
              { path: 'new', component: AdminBulletinCreatorComponent },
              { path: ':bulletinId', component: AdminBulletinEditorComponent },
            ]
          },
          {
            path: 'user',
            canActivate: [AdminGuard],
            data: { roles: ['Administrator', 'UserManager'] },
            children: [
              { path: '', pathMatch: 'full', component: AdminUserListComponent },
              { path: ':userId', component: AdminUserEditorComponent }
            ]
          },
          {
            path: 'contest',
            canActivate: [AdminGuard],
            data: { roles: ['Administrator', 'ContestManager'] },
            children: [
              { path: '', pathMatch: 'full', component: AdminContestListComponent },
              { path: 'new', component: AdminContestCreatorComponent },
              {
                path: ':contestId',
                component: AdminContestViewComponent,
                children: [
                  { path: '', pathMatch: 'full', component: AdminContestEditorComponent },
                  { path: 'registrations', component: AdminContestRegistrationsComponent }
                ]
              }
            ]
          },
          {
            path: 'problem',
            canActivate: [AdminGuard],
            data: { roles: ['Administrator', 'ContestManager'] },
            children: [
              { path: '', pathMatch: 'full', component: AdminProblemListComponent },
              { path: 'new', component: AdminProblemCreatorComponent },
              {
                path: ':problemId',
                component: AdminProblemViewComponent,
                children: [
                  { path: '', pathMatch: 'full', component: AdminProblemEditorComponent },
                  { path: 'tests', component: AdminProblemTestsComponent },
                  { path: 'export', component: AdminProblemExportComponent },
                  { path: 'plagiarisms', component: AdminProblemPlagiarismsComponent }
                ]
              }
            ]
          },
          {
            path: 'submission',
            canActivate: [AdminGuard],
            data: { roles: ['Administrator', 'SubmissionManager'] },
            children: [
              { path: '', pathMatch: 'full', component: AdminSubmissionListComponent },
              { path: 'rejudge', component: AdminSubmissionRejudgeComponent },
              { path: ':submissionId', component: AdminSubmissionEditorComponent }
            ]
          }
        ]
      }
    ]),
    ApiAuthorizationModule,
    NgbModule,
    FontAwesomeModule,
    MarkdownModule.forChild(),
    DatetimeModule.forChild(),
    EditorModule.forChild(),
    VditorModule.forChild(),
    VerdictModule.forChild()
  ],
  declarations: [
    AdminComponent,
    AdminDashboardComponent,
    AdminBulletinListComponent,
    AdminBulletinFormComponent,
    AdminBulletinCreatorComponent,
    AdminBulletinEditorComponent,
    AdminUserListComponent,
    AdminUserFormComponent,
    AdminUserEditorComponent,
    AdminContestListComponent,
    AdminContestViewComponent,
    AdminContestFormComponent,
    AdminContestCreatorComponent,
    AdminContestEditorComponent,
    AdminContestRegistrationsComponent,
    AdminProblemListComponent,
    AdminProblemViewComponent,
    AdminProblemFormComponent,
    AdminProblemCreatorComponent,
    AdminProblemEditorComponent,
    AdminProblemTestsComponent,
    AdminProblemExportComponent,
    AdminProblemPlagiarismsComponent,
    AdminSubmissionListComponent,
    AdminSubmissionFormComponent,
    AdminSubmissionEditorComponent,
    AdminSubmissionRejudgeComponent
  ],
  exports: [
    RouterModule
  ]
})
export class AdminModule {
}
