import { Injectable } from '@angular/core';
import { CanActivate } from '@angular/router';
import { OpenIdConnectService } from './open-id-connect.service';
import { environment } from '../../environments/environment';

@Injectable()
export class RequireAuthenticatedUserRouteGuardService implements CanActivate  {

  constructor(private openIdConnectService: OpenIdConnectService)
    { }

    canActivate()
    {
        if (this.openIdConnectService.userAvailable)
        {
            return true;
        }
        else
        {
            // trigger signin
            this.openIdConnectService.triggerSignIn();
            return false;
        }
    }
}
