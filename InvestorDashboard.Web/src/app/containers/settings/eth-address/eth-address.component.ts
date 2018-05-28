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
  constructor(private accountEndpoint: AccountEndpoint) {

  }

  ngOnInit() {
  }

  update() {
    this.errors = null;
    this.accountEndpoint.updateEthAddress(this.ethAddress).subscribe(data => {

    }, error => {
      console.log(error)
    });
  }

}
