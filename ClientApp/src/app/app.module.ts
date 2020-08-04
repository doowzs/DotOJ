import {NgModule} from '@angular/core';
import {FormsModule} from '@angular/forms';
import {HttpClientModule, HTTP_INTERCEPTORS} from '@angular/common/http';
import {RouterModule} from '@angular/router';

import {MonacoEditorModule} from 'ngx-monaco-editor';
import {MarkdownModule} from 'ngx-markdown';

import {AppComponent} from './app.component';
import {NavMenuComponent} from './nav-menu/nav-menu.component';
import {HomeComponent} from './home/home.component';
import {AssignmentListComponent} from './assignment/list/list.component';
import {AssignmentViewComponent} from './assignment/view/view.component';
import {AssignmentContentComponent} from './assignment/content/content.component';
import {ProblemListComponent} from './problem/list/list.component';
import {ProblemViewComponent} from './problem/view/view.component';

import {ApiAuthorizationModule} from 'src/api-authorization/api-authorization.module';
import {AuthorizeGuard} from 'src/api-authorization/authorize.guard';
import {AuthorizeInterceptor} from 'src/api-authorization/authorize.interceptor';

import {BrowserAnimationsModule} from '@angular/platform-browser/animations';
import {MatToolbarModule} from '@angular/material/toolbar';
import {MatButtonModule} from '@angular/material/button';
import {MatTableModule} from '@angular/material/table';
import {MatPaginatorModule} from '@angular/material/paginator';
import {MatTabsModule} from '@angular/material/tabs';
import {MatCardModule} from '@angular/material/card';
import {MatIconModule} from '@angular/material/icon';
import {MatProgressBarModule} from '@angular/material/progress-bar';

const routes = [
  {
    path: '',
    component: HomeComponent,
    pathMatch: 'full'
  },
  {
    path: 'assignments',
    component: AssignmentListComponent,
    canActivate: [AuthorizeGuard]
  },
  {
    path: 'assignment/:assignmentId',
    component: AssignmentViewComponent,
    canActivate: [AuthorizeGuard],
    children: [
      {
        path: '',
        component: AssignmentContentComponent,
        pathMatch: 'full'
      },
      {
        path: 'problem/:problemId',
        component: ProblemViewComponent
      }
    ]
  },
  {
    path: 'problems',
    component: ProblemListComponent,
    canActivate: [AuthorizeGuard]
  },
];

@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
    HomeComponent,
    AssignmentListComponent,
    AssignmentViewComponent,
    AssignmentContentComponent,
    ProblemListComponent,
    ProblemViewComponent
  ],
  imports: [
    HttpClientModule,
    FormsModule,
    ApiAuthorizationModule,
    MarkdownModule.forRoot(),
    MonacoEditorModule.forRoot(),
    RouterModule.forRoot(routes),
    BrowserAnimationsModule,
    MatToolbarModule,
    MatButtonModule,
    MatTableModule,
    MatPaginatorModule,
    MatTabsModule,
    MatCardModule,
    MatIconModule,
    MatProgressBarModule
  ],
  providers: [
    {provide: HTTP_INTERCEPTORS, useClass: AuthorizeInterceptor, multi: true}
  ],
  bootstrap: [AppComponent]
})
export class AppModule {
}
