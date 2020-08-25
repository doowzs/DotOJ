import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { APP_INITIALIZER, NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { RouterModule } from '@angular/router';

import { AuthorizeGuard } from 'src/api-authorization/authorize.guard';
import { AuthorizeInterceptor } from 'src/api-authorization/authorize.interceptor';
import { ApiAuthorizationModule } from 'src/api-authorization/api-authorization.module';

import { AppComponent } from 'src/app/app.component';
import { ApplicationConfigService } from 'src/app/services/config.service';
import { ApplicationApiInterceptor } from 'src/app/services/api.interceptor';
import { MainHeaderComponent } from './components/headers/main/main.component';
import { MainFooterComponent } from './components/footers/main/main.component';
import { WelcomeComponent } from './components/welcome/welcome.component';
import { ContestListComponent } from './components/contest/list/list.component';
import { ContestViewComponent } from './components/contest/view/view.component';
import { ContestDescriptionComponent } from './components/contest/description/description.component';

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

const loadApplicationConfig = (service: ApplicationConfigService) => {
  return () => service.loadApplicationConfig();
};

@NgModule({
  declarations: [
    AppComponent,
    MainHeaderComponent,
    MainFooterComponent,
    WelcomeComponent,
    ContestListComponent,
    ContestViewComponent,
    ContestDescriptionComponent
  ],
  imports: [
    BrowserAnimationsModule,
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    FormsModule,
    HttpClientModule,
    RouterModule.forRoot([
      { path: '', pathMatch: 'full', component: WelcomeComponent },
      { path: 'contests', component: ContestListComponent },
      {
        path: 'contest/:contestId', component: ContestViewComponent, children: [
          { path: '', pathMatch: 'full', component: ContestDescriptionComponent }
        ]
      }
    ]),
    ApiAuthorizationModule,
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
  ],
  providers: [
    ApplicationConfigService,
    { provide: APP_INITIALIZER, useFactory: loadApplicationConfig, multi: true, deps: [ApplicationConfigService] },
    { provide: HTTP_INTERCEPTORS, useClass: ApplicationApiInterceptor, multi: true },
    { provide: HTTP_INTERCEPTORS, useClass: AuthorizeInterceptor, multi: true }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
