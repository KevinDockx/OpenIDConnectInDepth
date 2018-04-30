import { Component } from '@angular/core';
import {  } from "automapper-ts";
import { OpenIdConnectService } from './shared/open-id-connect.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  title = 'OpenID Connect In Depth Demo';  

  constructor(private openIdConnectService: OpenIdConnectService) {
  }
 
}



