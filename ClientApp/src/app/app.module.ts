import { BrowserModule } from '@angular/platform-browser';
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
import { WelcomeComponent } from './components/welcome/welcome.component';

import { NzLayoutModule } from 'ng-zorro-antd/layout';

const loadApplicationConfig = (service: ApplicationConfigService) => {
  return () => service.loadApplicationConfig();
};

@NgModule({
  declarations: [
    AppComponent,
    WelcomeComponent
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    ApiAuthorizationModule,
    RouterModule.forRoot([
      {path: '', pathMatch: 'full', component: WelcomeComponent}
    ]),
    NzLayoutModule
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
