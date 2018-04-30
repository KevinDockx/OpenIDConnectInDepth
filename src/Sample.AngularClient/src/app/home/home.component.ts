import { Component, OnInit, ErrorHandler } from '@angular/core';

import { DataService } from './shared/data.service';
import { OpenIdConnectService } from '../shared/open-id-connect.service';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit {
  title: string = 'Sample Angular Client'
  claims: string[] = [];

  constructor(private openIdConnectService: OpenIdConnectService, 
    private dataService: DataService) { }

  callApi()
  {
    this.dataService.getClaims()
    .subscribe(claimsFromApi => {
        this.claims =  this.claims.concat(this.claims, claimsFromApi);
    });    
  }

  ngOnInit() {

    debugger;
    let properties = Object.getOwnPropertyNames(this.openIdConnectService.user.profile);
    for (let property of properties)
    {
      if (this.openIdConnectService.user.profile.hasOwnProperty(property)) 
      {
          this.claims.push(property + " : " + this.openIdConnectService.user.profile[property]);
      }
    }
  }

}
