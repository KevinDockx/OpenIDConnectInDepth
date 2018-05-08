import { Component, OnInit, ErrorHandler, OnDestroy } from "@angular/core";

import { DataService } from "./shared/data.service";
import { OpenIdConnectService } from "../shared/open-id-connect.service";
import { Subject } from "rxjs/Subject";
import "rxjs/add/operator/takeUntil";

@Component({
  selector: "app-home",
  templateUrl: "./home.component.html",
  styleUrls: ["./home.component.css"]
})
export class HomeComponent implements OnInit, OnDestroy {
  private ngUnsubscribe: Subject<void> = new Subject();
  title: string = "Sample Angular Client";
  claims: string[] = [];

  constructor(
    private openIdConnectService: OpenIdConnectService,
    private dataService: DataService
  ) {}

  callApi() {
    this.dataService
      .getClaims()
      .takeUntil(this.ngUnsubscribe)
      .subscribe(claimsFromApi => {
        this.claims = this.claims.concat(this.claims, claimsFromApi);
      });
  }

  ngOnDestroy() {
    this.ngUnsubscribe.next();
    this.ngUnsubscribe.complete();
  }

  ngOnInit() {
    Object.keys(this.openIdConnectService.user.profile).forEach(property => {
      this.claims.push(
        `${property} : ${this.openIdConnectService.user.profile[property]}`
      );
    });
  }
}
