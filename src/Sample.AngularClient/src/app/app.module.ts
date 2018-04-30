import { BrowserModule } from '@angular/platform-browser';
import { NgModule, ErrorHandler } from '@angular/core';
import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { DatePipe } from '@angular/common'
import { HttpClient } from '@angular/common/http/src/client';

// import routing module
import { AppRoutingModule } from './app.routing';
import { AppComponent } from './app.component';
import { AboutComponent } from './about';
import { HomeComponent } from './home';

import { DataService } from './home/shared/data.service';

import { GlobalErrorHandler } from './shared/global-error-handler';
import { ErrorLoggerService } from './shared/error-logger.service';
// import { HandleHttpErrorInterceptor } from './shared/handle-http-error-interceptor';
import { WriteOutJsonInterceptor } from './shared/write-out-json-interceptor';
import { OpenIdConnectService } from './shared/open-id-connect.service';
import { SigninOidcComponent } from './signin-oidc/signin-oidc.component';
import { RequireAuthenticatedUserRouteGuardService } from './shared/require-authenticated-user-route-guard.service';
import { AddAuthorizationHeaderInterceptor } from './shared/add-authorization-header-interceptor';
import { RedirectSilentRenewComponent } from './redirect-silent-renew/redirect-silent-renew.component';

@NgModule({
  declarations: [
    AppComponent,
    AboutComponent,
    HomeComponent,  
    SigninOidcComponent,
    RedirectSilentRenewComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    HttpClientModule,
    FormsModule,
    ReactiveFormsModule
  ],
  providers: [    
    {
      provide: HTTP_INTERCEPTORS,
      useClass: AddAuthorizationHeaderInterceptor,
      multi: true
    },  
    {
      provide: HTTP_INTERCEPTORS,
      useClass: WriteOutJsonInterceptor,
      multi: true
    },
    // {
    //   provide: HTTP_INTERCEPTORS,
    //   useClass: HandleHttpErrorInterceptor,
    //   multi: true,
    // },
    GlobalErrorHandler, ErrorLoggerService, DataService, 
    OpenIdConnectService, RequireAuthenticatedUserRouteGuardService],
  bootstrap: [AppComponent]
})
export class AppModule {

  constructor() {
  }
}
