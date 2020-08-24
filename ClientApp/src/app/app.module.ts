import { BrowserModule } from '@angular/platform-browser';
import { APP_INITIALIZER, NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { RouterModule } from '@angular/router';

import { AuthorizeGuard } from 'src/api-authorization/authorize.guard';
import { AuthorizeInterceptor } from 'src/api-authorization/authorize.interceptor';
import { ApiAuthorizationModule } from 'src/api-authorization/api-authorization.module';

import { AppComponent } from 'src/app/app.component';
import { ApplicationConfigService } from 'src/app/app.config.service';
import { ApplicationApiInterceptor } from 'src/app/app.api.interceptor';

const loadApplicationConfig = (service: ApplicationConfigService) => {
  return () => service.loadApplicationConfig();
};

@NgModule({
  declarations: [
    AppComponent,
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    ApiAuthorizationModule,
    RouterModule.forRoot([])
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
