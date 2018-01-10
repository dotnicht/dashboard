import { Component, ViewChild, OnInit } from '@angular/core';
import { AppTranslationService } from '../../services/app-translation.service';
import { ReCaptchaComponent } from 'angular2-recaptcha';
import { TokenTransfer } from '../../models/tokenTransfer.model';
import { DashboardEndpoint } from '../../services/dashboard-endpoint.service';
import { AuthService } from '../../services/auth.service';
import { Dashboard } from '../../models/dashboard.models';
import { MatCheckboxChange } from '@angular/material';
import { error } from 'selenium-webdriver';
const ethereum_address = require('ethereum-address');

@Component({
    selector: 'app-transfer',
    templateUrl: './transfer.component.html',
    styleUrls: ['./transfer.component.scss']
})
/** transfer component*/
export class TransferComponent implements OnInit {


    dashboard: Dashboard;
    transfer: TokenTransfer;
    isLoading = false;
    sendEntire = false;
    errors: string;


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

        console.log(this.transfer);

        this.dashboardService.addtokenTransfer(this.transfer).subscribe(resp => {
            this.transfer.amount = 0;
        },
            error => {
                console.log(error);
            });
    }
    ngOnInit() {
        this.transfer.address = '0xde0B295669a9FD93d5F28D9Ec85E40f4cb697BAe';
        this.dashboardService.getDashboard().subscribe(model => {
            const db = model.json() as Dashboard;
            // db.clientInfoModel.balance = 4;
            // db.clientInfoModel.bonusBalance = 50;
            this.dashboard = db;
        });
        console.log(this.dashboard);
    }

    checkSendEntire(check: MatCheckboxChange) {
        if (check.checked) {
            this.sendEntire = true;
            this.transfer.amount = this.dashboard.clientInfoModel.balance + this.dashboard.clientInfoModel.bonusBalance;
        } else {
            this.sendEntire = false;
            this.transfer.amount = 0;
        }
    }
    validateValue() {
        if (this.transfer.amount > (this.dashboard.clientInfoModel.balance + this.dashboard.clientInfoModel.bonusBalance)) {
            this.errors = 'Not enough tokens';
        } else {
            this.errors = undefined;
        }
        console.log(this.transfer.amount);
    }
    get validAmount() {
        // if(this.transfer.amount > 0){

        // }
        const valid = this.transfer.amount > 0 && (this.transfer.amount <= (this.dashboard.clientInfoModel.balance + this.dashboard.clientInfoModel.bonusBalance));
        return valid;
    }

}
