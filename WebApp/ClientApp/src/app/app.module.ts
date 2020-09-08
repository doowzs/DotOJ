import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { APP_INITIALIZER, NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { RouterModule } from '@angular/router';

import { AuthorizeGuard } from 'src/api-authorization/authorize.guard';
import { AuthorizeInterceptor } from 'src/api-authorization/authorize.interceptor';
import { ApiAuthorizationModule } from 'src/api-authorization/api-authorization.module';
import { AdminModule } from 'src/admin/admin.module';

import { NoCommaPipe } from './pipes/no-comma.pipe';
import { AppComponent } from './app.component';
import { ApplicationConfigService } from './services/config.service';
import { ApplicationApiInterceptor } from './services/api.interceptor';
import { ChangelogComponent } from './components/changelog/changelog.component';
import { MainHeaderComponent } from './components/headers/main/main.component';
import { ContestHeaderComponent } from './components/headers/contest/contest.component';
import { MainFooterComponent } from './components/footers/main/main.component';
import { WelcomePageComponent } from './components/welcome/page/page.component';
import { WelcomeBulletinsComponent } from './components/welcome/bulletins/bulletins.component';
import { WelcomeContestsComponent } from './components/welcome/contests/contests.component';
import { ContestListComponent } from './components/contest/list/list.component';
import { ContestViewComponent } from './components/contest/view/view.component';
import { ContestRuleComponent } from './components/contest/rule/rule.component';
import { ContestDescriptionComponent } from './components/contest/description/description.component';
import { ContestStandingsComponent } from './components/contest/standings/standings.component';
import { ProblemDetailComponent } from './components/problem/detail/detail.component';
import { SubmissionListComponent } from './components/submission/list/list.component';
import { SubmissionCreatorComponent } from './components/submission/creator/creator.component';
import { SubmissionVerdictComponent } from './components/submission/verdict/verdict.component';
import { SubmissionTimelineComponent } from './components/submission/timeline/timeline.component';
import { SubmissionDetailComponent } from './components/submission/detail/detail.component';

import { MarkdownModule } from '../lib/markdown/markdown.module';
import { en_US, NZ_I18N } from 'ng-zorro-antd/i18n';
import { NzLayoutModule } from 'ng-zorro-antd/layout';
import { NzPageHeaderModule } from 'ng-zorro-antd/page-header';
import { NzGridModule } from 'ng-zorro-antd/grid';
import { NzCardModule } from 'ng-zorro-antd/card';
import { NzTagModule } from 'ng-zorro-antd/tag';
import { NzStatisticModule } from 'ng-zorro-antd/statistic';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzDropDownModule } from 'ng-zorro-antd/dropdown';
import { NzListModule } from 'ng-zorro-antd/list';
import { NzPaginationModule } from 'ng-zorro-antd/pagination';
import { NzDividerModule } from 'ng-zorro-antd/divider';
import { NzDescriptionsModule } from 'ng-zorro-antd/descriptions';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { NzSelectModule } from 'ng-zorro-antd/select';
import { NzToolTipModule } from 'ng-zorro-antd/tooltip';
import { NzNotificationModule } from 'ng-zorro-antd/notification';
import { NzTimelineModule } from 'ng-zorro-antd/timeline';
import { NzTableModule } from 'ng-zorro-antd/table';
import { NzEmptyModule } from 'ng-zorro-antd/empty';
import { NzDrawerModule } from 'ng-zorro-antd/drawer';
import { NzSkeletonModule } from 'ng-zorro-antd/skeleton';
import { NzBadgeModule } from 'ng-zorro-antd/badge';
import { NzModalModule } from 'ng-zorro-antd/modal';
import { NzInputModule } from 'ng-zorro-antd/input';

const loadApplicationConfig = (service: ApplicationConfigService) => {
  return () => service.loadApplicationConfig();
};

@NgModule({
  declarations: [
    NoCommaPipe,
    AppComponent,
    ChangelogComponent,
    MainHeaderComponent,
    ContestHeaderComponent,
    MainFooterComponent,
    WelcomePageComponent,
    WelcomeBulletinsComponent,
    WelcomeContestsComponent,
    ContestListComponent,
    ContestViewComponent,
    ContestRuleComponent,
    ContestDescriptionComponent,
    ContestStandingsComponent,
    ProblemDetailComponent,
    SubmissionListComponent,
    SubmissionCreatorComponent,
    SubmissionVerdictComponent,
    SubmissionTimelineComponent,
    SubmissionDetailComponent
  ],
  imports: [
    BrowserAnimationsModule,
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    FormsModule,
    HttpClientModule,
    RouterModule.forRoot([
      { path: '', pathMatch: 'full', component: WelcomePageComponent },
      { path: 'changelog', component: ChangelogComponent },
      { path: 'contests', component: ContestListComponent, canActivate: [AuthorizeGuard] },
      {
        path: 'contest/:contestId', component: ContestViewComponent, canActivate: [AuthorizeGuard],
        children: [
          { path: '', pathMatch: 'full', component: ContestDescriptionComponent },
          { path: 'problem/:problemId', component: ProblemDetailComponent },
          { path: 'submissions', component: SubmissionListComponent },
          { path: 'standings', component: ContestStandingsComponent }
        ]
      }
    ]),
    AdminModule,
    ApiAuthorizationModule,
    MarkdownModule.forRoot(),
    NzLayoutModule,
    NzPageHeaderModule,
    NzGridModule,
    NzCardModule,
    NzTagModule,
    NzStatisticModule,
    NzButtonModule,
    NzDropDownModule,
    NzListModule,
    NzPaginationModule,
    NzDividerModule,
    NzDescriptionsModule,
    NzIconModule,
    NzSelectModule,
    NzToolTipModule,
    NzNotificationModule,
    NzTimelineModule,
    NzTableModule,
    NzEmptyModule,
    NzDrawerModule,
    NzSkeletonModule,
    NzBadgeModule,
    NzModalModule,
    NzInputModule
  ],
  providers: [
    ApplicationConfigService,
    { provide: APP_INITIALIZER, useFactory: loadApplicationConfig, multi: true, deps: [ApplicationConfigService] },
    { provide: HTTP_INTERCEPTORS, useClass: ApplicationApiInterceptor, multi: true },
    { provide: HTTP_INTERCEPTORS, useClass: AuthorizeInterceptor, multi: true },
    { provide: NZ_I18N, useValue: en_US }
  ],
  bootstrap: [AppComponent]
})
export class AppModule {
}
