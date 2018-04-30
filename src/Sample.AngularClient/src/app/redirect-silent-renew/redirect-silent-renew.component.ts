import { Component, OnInit } from '@angular/core';
import { OpenIdConnectService } from '../shared/open-id-connect.service';

@Component({
  selector: 'app-redirect-silent-renew',
  templateUrl: './redirect-silent-renew.component.html',
  styleUrls: ['./redirect-silent-renew.component.css']
})
export class RedirectSilentRenewComponent implements OnInit {

  constructor(private openIdConnectService : OpenIdConnectService) { }

  ngOnInit() {
    this.openIdConnectService.handleSilentCallback();
  }

}
