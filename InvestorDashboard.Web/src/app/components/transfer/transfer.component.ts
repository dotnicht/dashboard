import { Component, ViewChild, OnInit } from '@angular/core';
import { AppTranslationService } from '../../services/app-translation.service';
import { ReCaptchaComponent } from 'angular2-recaptcha';
import { TokenTransfer } from '../../models/tokenTransfer.model';
import { DashboardEndpoint } from '../../services/dashboard-endpoint.service';
const ethereum_address = require('ethereum-address');

@Component({
    selector: 'app-transfer',
    templateUrl: './transfer.component.html',
    styleUrls: ['./transfer.component.scss']
})
/** transfer component*/
export class TransferComponent implements OnInit {

    @ViewChild(ReCaptchaComponent) captcha: ReCaptchaComponent;
    transfer: TokenTransfer;
    isLoading = false;
    reCaptchaStatus = false;

    constructor(private translationService: AppTranslationService,
        private dashboardService: DashboardEndpoint) {

        this.transfer = new TokenTransfer(1);

    }
    validateAddress(value: string) {
        if (ethereum_address.isAddress(value)) {
            console.log('Valid ethereum address.');
        }
        else {
            console.log('Invalid Ethereum address.');
        }
    }
    OnSubmit() {

        this.transfer.reCaptchaToken = this.captcha.getResponse();
        console.log(this.transfer);

        this.dashboardService.addtokenTransfer(this.transfer).subscribe(resp => {
            this.transfer.amount = 1;
            this.captcha.reset();
            this.reCaptchaStatus = false;
        },
            error => {
                console.log(error);
            });
    }
    ngOnInit() {
        this.transfer.address = '0xde0B295669a9FD93d5F28D9Ec85E40f4cb697BAe';
    }
    handleCorrectCaptcha(event) {
        this.reCaptchaStatus = true;
    }
    handleCaptchaExpired() {
        this.reCaptchaStatus = false;
    }
}
