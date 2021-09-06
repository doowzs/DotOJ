import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { APP_INITIALIZER, NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { RouterModule } from '@angular/router';

import { AuthModule} from "src/auth/auth.module";
import { AdminModule } from 'src/admin/admin.module';

import { NoCommaPipe } from './pipes/no-comma.pipe';
import { AppComponent } from './app.component';
import { ApplicationConfigService } from './services/config.service';
import { ApplicationApiInterceptor } from './services/api.interceptor';
import { MainHeaderComponent } from './components/headers/main/main.component';
import { ContestHeaderComponent } from './components/headers/contest/contest.component';
import { MainFooterComponent } from './components/footers/main/main.component';
import { WelcomePageComponent } from './components/welcome/page/page.component';
import { WelcomeExamComponent } from './components/welcome/exam/exam.component';
import { WelcomeBulletinsComponent } from './components/welcome/bulletins/bulletins.component';
import { WelcomeContestsComponent } from './components/welcome/contests/contests.component';
import { ContestListComponent } from './components/contest/list/list.component';
import { ContestViewComponent } from './components/contest/view/view.component';
import { ContestDescriptionComponent } from './components/contest/description/description.component';
import { ContestSubmissionsComponent } from './components/contest/submissions/submissions.component';
import { ContestStandingsComponent } from './components/contest/standings/standings.component';
import { ProblemDetailComponent } from './components/problem/detail/detail.component';
import { SubmissionListComponent } from './components/submission/list/list.component';
import { SubmissionOrdinaryCreatorComponent } from './components/submission/creators/ordinary/ordinary.component';
import { SubmissionTestKitCreatorComponent } from "./components/submission/creators/testkit/testkit.component";
import { SubmissionTimelineComponent } from './components/submission/timeline/timeline.component';
import { SubmissionDetailComponent } from './components/submission/detail/detail.component';
import { HelpComponent } from './components/miscellaneous/help/help.component';
import { ChangelogComponent } from './components/miscellaneous/changelog/changelog.component';

import { ClipboardModule } from 'ngx-clipboard';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { ChartsModule } from 'ng2-charts';
import { MarkdownModule } from '../lib/markdown/markdown.module';
import { EditorModule } from '../lib/editor/editor.module';
import { VerdictModule } from '../lib/verdict/verdict.module';
import {AuthorizeGuard} from "../auth/authorize.guard";
import {AuthorizeInterceptor} from "../auth/authorize.interceptor";

const loadApplicationConfig = (service: ApplicationConfigService) => {
  return () => service.loadApplicationConfig();
};

@NgModule({
  declarations: [
    NoCommaPipe,
    AppComponent,
    MainHeaderComponent,
    ContestHeaderComponent,
    MainFooterComponent,
    WelcomePageComponent,
    WelcomeExamComponent,
    WelcomeBulletinsComponent,
    WelcomeContestsComponent,
    ContestListComponent,
    ContestViewComponent,
    ContestDescriptionComponent,
    ContestSubmissionsComponent,
    ContestStandingsComponent,
    ProblemDetailComponent,
    SubmissionListComponent,
    SubmissionOrdinaryCreatorComponent,
    SubmissionTestKitCreatorComponent,
    SubmissionTimelineComponent,
    SubmissionDetailComponent,
    HelpComponent,
    ChangelogComponent
  ],
  imports: [
    BrowserAnimationsModule,
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    FormsModule,
    HttpClientModule,
    RouterModule.forRoot([
      { path: '', pathMatch: 'full', component: WelcomePageComponent },
      { path: 'help', component: HelpComponent },
      { path: 'changelog', component: ChangelogComponent },
      { path: 'contests', component: ContestListComponent, canActivate: [AuthorizeGuard] },
      {
        path: 'contest/:contestId', component: ContestViewComponent, canActivate: [AuthorizeGuard],
        children: [
          { path: '', pathMatch: 'full', component: ContestDescriptionComponent },
          { path: 'problem/:problemId', component: ProblemDetailComponent },
          { path: 'submissions', component: ContestSubmissionsComponent },
          { path: 'standings', component: ContestStandingsComponent }
        ]
      },
      { path: 'submissions', component: SubmissionListComponent, canActivate: [AuthorizeGuard] },
      { path: 'submission/:submissionId', component: SubmissionDetailComponent, canActivate: [AuthorizeGuard] }
    ]),
    AuthModule,
    AdminModule,
    ClipboardModule,
    NgbModule,
    FontAwesomeModule,
    ChartsModule,
    EditorModule.forRoot(),
    MarkdownModule.forRoot(),
    VerdictModule.forRoot()
  ],
  providers: [
    ApplicationConfigService,
    { provide: APP_INITIALIZER, useFactory: loadApplicationConfig, multi: true, deps: [ApplicationConfigService] },
    { provide: HTTP_INTERCEPTORS, useClass: ApplicationApiInterceptor, multi: true },
    { provide: HTTP_INTERCEPTORS, useClass: AuthorizeInterceptor, multi: true },
  ],
  bootstrap: [AppComponent]
})
export class AppModule {
}
