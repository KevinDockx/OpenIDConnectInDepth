import { Component, OnInit, OnDestroy } from "@angular/core";
import { OpenIdConnectService } from "../shared/open-id-connect.service";
import { Router } from "@angular/router";
import { Subject } from "rxjs/Subject";
import "rxjs/add/operator/takeUntil";

@Component({
  selector: "app-signin-oidc",
  templateUrl: "./signin-oidc.component.html",
  styleUrls: ["./signin-oidc.component.css"]
})
export class SigninOidcComponent implements OnInit, OnDestroy {
  private ngUnsubscribe: Subject<void> = new Subject();
  constructor(
    private openIdConnectService: OpenIdConnectService,
    private router: Router
  ) {}

  ngOnDestroy() {
    this.ngUnsubscribe.next();
    this.ngUnsubscribe.complete();
  }

  ngOnInit() {
    this.openIdConnectService.userLoaded$
      .takeUntil(this.ngUnsubscribe)
      .subscribe(userLoaded => {
        if (userLoaded) {
          this.router.navigate(["./"]);
        }
      });

    this.openIdConnectService.handleCallback();
  }
}
