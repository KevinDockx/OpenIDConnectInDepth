import { Routes, RouterModule } from '@angular/router';

import { AboutComponent } from './about'; 
import { AppComponent } from './app.component';
import { HomeComponent } from './home';

import { NgModule } from '@angular/core';
import { SigninOidcComponent } from './signin-oidc/signin-oidc.component';
import { RequireAuthenticatedUserRouteGuardService } from './shared/require-authenticated-user-route-guard.service';
import { RedirectSilentRenewComponent } from './redirect-silent-renew/redirect-silent-renew.component';

const routes: Routes = [
    { path: '', redirectTo: 'home', pathMatch: 'full', 
        canActivate: [RequireAuthenticatedUserRouteGuardService] },
    { path: 'home', component: HomeComponent, 
        canActivate: [RequireAuthenticatedUserRouteGuardService] },
    { path: 'about', component: AboutComponent },   
    { path: 'signin-oidc', component: SigninOidcComponent },
    { path: 'redirect-silentrenew', component: RedirectSilentRenewComponent }
];

// define a module
@NgModule({
    imports: [RouterModule.forRoot(routes)],
    exports: [RouterModule]
})

export class AppRoutingModule { }
