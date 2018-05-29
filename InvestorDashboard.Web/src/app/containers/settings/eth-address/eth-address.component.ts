import { Component, OnInit } from '@angular/core';
import { AccountEndpoint } from '../../../services/account-endpoint.service';

@Component({
  selector: 'app-eth-address',
  templateUrl: './eth-address.component.html',
  styleUrls: ['./eth-address.component.scss']
})
export class EthAddressComponent implements OnInit {
  ethAddress: string;
  errors: string;
  saved = false;
  constructor(private accountEndpoint: AccountEndpoint) {

  }

  ngOnInit() {
    this.accountEndpoint.getEthAddress().subscribe(data => {
      this.ethAddress = data.json() as string;
    }, error => {
      console.log(error)
    });
  }

  update() {
    this.errors = null;
    this.saved = false;
    this.accountEndpoint.updateEthAddress(this.ethAddress).subscribe(data => {
      this.saved = true;
    }, error => {
      console.log(error)
    });
  }

}
