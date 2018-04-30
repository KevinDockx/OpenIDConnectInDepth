import { Injectable } from '@angular/core';
import { UserManager, User } from 'oidc-client';
import { environment } from '../../environments/environment';
import { ReplaySubject } from 'rxjs/ReplaySubject';

@Injectable()
export class OpenIdConnectService {
   
// instantiate the manager
private userManager: UserManager = new UserManager(environment.openIdConnectSettings);

// the current user
private currentUser: User;

// // ReplaySubject for subcribing (instead of EventEmitter, which shouldn't 
// // be used inside of a service). 
// //
// // A Subject is both an Observable (can subscribe to it)
// // and an Observer (can call next on it to emit a new value). 
// // 
// // A ReplaySubject is a variant of Subject. 
// // If you want to wait until a value is actually produced, use ReplaySubject(1).
// // ReplaySubject will always provide the most recent value, but since it does not have a required initial value, 
// // the service can do some async operation before returning it's first value (that's what we're doing when
// // loading the user). It will still fire immediately on subsequent calls with the most recent value (which we use
// // in the Guard).
userLoaded$ = new ReplaySubject<boolean>(1);


// returns true if a user has been loaded - used in the Guard
get userAvailable() : boolean {
    return this.currentUser != null;
}

// returns the current user.  Can be used when profile information is
// required.  Note that, if required, this can be removed in favour of
// a "userProfile" var that only exposed the fields from the user's 
// profile fields that are required by calling components instead of exposing
// the full profile of the UserManagers' current user.
get user(): User {
    return this.currentUser;
}

constructor() { //private http: Http) {

    // clears stale state that might be left over from previous sessions
    this.userManager.clearStaleState();

    // handle the event that's raised when a user is loaded
    // (ie: when a user session has been established (or re-established, eg: from storage)).
    this.userManager.events.addUserLoaded(user => {

        // A user can be loaded, but this says nothing about his
        // session status at level of the IDP (could come from storage).
        //
        // Logged in = logged in at level of the 
        // IDP, not in our app, as there's no such thing as being
        // "logged in" to a user-agent based app.  We use the 
        // "userAvailable" value for checking if a user has been loaded.

        if (!environment.production) {
            console.log('User loaded.', user);
        }
        this.currentUser = user;
        this.userLoaded$.next(true);
    });

    // handle the event that's raised when a user is unloaded  
    // (ie: when a user session has been terminated).
    this.userManager.events.addUserUnloaded((e) => {
        if (!environment.production) {
            console.log('User unloaded.');
        }
        this.currentUser = null;
        this.userLoaded$.next(false);
    });

    // handle the event that's raised when the user's signin status 
    // at the IDP has changed. 
    this.userManager.events.addUserSignedOut((e) => {
        debugger;
        if (!environment.production) {
            console.log('User signed out.');
        }
        this.currentUser = null;
        this.userLoaded$.next(false);
    });
} 

triggerSignIn() {
    this.userManager.signinRedirect().then(function () {
        if (!environment.production) {
            console.log('Redirection to signin triggered.');
        }
    });
}

handleCallback() {
    this.userManager.signinRedirectCallback().then(function (user) {
        if (!environment.production) {
            console.log('Callback after signin handled.', user);
        }
    });
}

handleSilentCallback() {
    this.userManager.signinSilentCallback().then(function (user) {       
        this.currentUser = user
        if (!environment.production) {
            console.log('Callback after silent signin handled.', user);
        }       
   });
}

triggerSignOut() {
    this.userManager.signoutRedirect().then(function (resp) {
        if (!environment.production) {
            console.log('Redirection to sign out triggered.', resp);
        }
    });
};
}
