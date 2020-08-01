import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { RouterModule } from '@angular/router';

import { MonacoEditorModule } from 'ngx-monaco-editor';

import { AppComponent } from './app.component';
import { NavMenuComponent } from './nav-menu/nav-menu.component';
import { HomeComponent } from './home/home.component';
import { AssignmentListComponent } from './assignment-list/assignment-list.component';
import { AssignmentViewComponent } from './assignment-view/assignment-view.component';
import { ProblemListComponent } from './problem-list/problem-list.component';
import { ApiAuthorizationModule } from 'src/api-authorization/api-authorization.module';
import { AuthorizeGuard } from 'src/api-authorization/authorize.guard';
import { AuthorizeInterceptor } from 'src/api-authorization/authorize.interceptor';
import {BrowserAnimationsModule} from '@angular/platform-browser/animations';
import {MatToolbarModule} from '@angular/material/toolbar';
import {MatButtonModule} from '@angular/material/button';
import {MatTableModule} from '@angular/material/table';

@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
    HomeComponent,
    AssignmentListComponent,
    ProblemListComponent,
  ],
  imports: [
    HttpClientModule,
    FormsModule,
    ApiAuthorizationModule,
    MonacoEditorModule.forRoot(),
    RouterModule.forRoot([
      {path: '', component: HomeComponent, pathMatch: 'full'},
      {path: 'assignments', component: AssignmentListComponent, canActivate: [AuthorizeGuard]},
      {path: 'assignment/:id', component: AssignmentViewComponent, canActivate: [AuthorizeGuard]},
      {path: 'problems', component: ProblemListComponent, canActivate: [AuthorizeGuard]},
    ]),
    BrowserAnimationsModule,
    MatToolbarModule,
    MatButtonModule,
    MatTableModule
  ],
  providers: [
    { provide: HTTP_INTERCEPTORS, useClass: AuthorizeInterceptor, multi: true }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
