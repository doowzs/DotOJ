import {NgModule} from '@angular/core';
import {BrowserModule} from '@angular/platform-browser';
import {FormsModule} from '@angular/forms';
import {HttpClientModule, HTTP_INTERCEPTORS} from '@angular/common/http';
import {RouterModule} from '@angular/router';
import {MarkdownModule} from 'ngx-markdown';

import {AppComponent} from './app.component';
import {NavMenuComponent} from './nav-menu/nav-menu.component';
import {HomeComponent} from './home/home.component';
import {AssignmentListComponent} from './assignment/list/list.component';
import {AssignmentViewComponent} from './assignment/view/view.component';
import {AssignmentContentComponent} from './assignment/content/content.component';
import {ProblemListComponent} from './problem/list/list.component';
import {ProblemViewComponent} from './problem/view/view.component';
import {ProblemContentComponent} from './problem/content/content.component';
import {ProblemCodeEditorComponent} from './problem/editor/editor.component';
import {CodeEditorDirective} from './problem/editor/editor.directive';
import {ProblemSubmissionsComponent} from './submission/problem/problem.component';

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
import {MatGridListModule} from '@angular/material/grid-list';
import {MatSelectModule} from '@angular/material/select';
import {MatSnackBarModule} from '@angular/material/snack-bar';
import {MatSlideToggleModule} from '@angular/material/slide-toggle';
import {MatListModule} from "@angular/material/list";

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
    ProblemViewComponent,
    ProblemContentComponent,
    ProblemCodeEditorComponent,
    CodeEditorDirective,
    ProblemSubmissionsComponent
  ],
  imports: [
    BrowserModule,
    HttpClientModule,
    FormsModule,
    ApiAuthorizationModule,
    MarkdownModule.forRoot(),
    RouterModule.forRoot(routes),
    BrowserAnimationsModule,
    MatToolbarModule,
    MatButtonModule,
    MatTableModule,
    MatPaginatorModule,
    MatTabsModule,
    MatCardModule,
    MatIconModule,
    MatProgressBarModule,
    MatGridListModule,
    MatSelectModule,
    MatSnackBarModule,
    MatSlideToggleModule,
    MatListModule
  ],
  providers: [
    {provide: HTTP_INTERCEPTORS, useClass: AuthorizeInterceptor, multi: true}
  ],
  bootstrap: [AppComponent]
})
export class AppModule {
}
