import { Component, ViewChild, OnInit, Inject } from '@angular/core';
import { AppTranslationService } from '../../services/app-translation.service';
import { ReCaptchaComponent } from 'angular2-recaptcha';
import { TokenTransfer } from '../../models/tokenTransfer.model';
import { DashboardEndpoint } from '../../services/dashboard-endpoint.service';
import { AuthService } from '../../services/auth.service';
import { Dashboard } from '../../models/dashboard.models';
import { MatCheckboxChange, MatDialogRef, MatDialog } from '@angular/material';
import { error } from 'selenium-webdriver';
import { DOCUMENT } from '@angular/platform-browser';
const ethereum_address = require('ethereum-address');

@Component({
    selector: 'app-transfer',
    templateUrl: './transfer.component.html',
    styleUrls: ['./transfer.component.scss']
})
/** transfer component*/
export class TransferComponent implements OnInit {

    succsessTransferDialogRef: MatDialogRef<SuccsessTransferDialogComponent> | null;
    failedTransferDialogRef: MatDialogRef<FailedTransferDialogComponent> | null;

    dashboard: Dashboard;
    transfer: TokenTransfer;
    isLoading = false;
    sendEntire = false;
    errors: string;


    constructor(private translationService: AppTranslationService,
        private dashboardService: DashboardEndpoint,
        private dialog: MatDialog,
        @Inject(DOCUMENT) doc: any) {
        dialog.afterOpen.subscribe(() => {
            if (!doc.body.classList.contains('no-scroll')) {
                doc.body.classList.add('no-scroll');
            }
        });
        dialog.afterAllClosed.subscribe(() => {
            doc.body.classList.remove('no-scroll');
        });
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
            this.succsessTransferDialogRef = this.dialog.open(SuccsessTransferDialogComponent);
        },
            error => {
                console.log(error);
                this.failedTransferDialogRef = this.dialog.open(FailedTransferDialogComponent);
            });
    }
    ngOnInit() {
        this.transfer.address = '0xde0B295669a9FD93d5F28D9Ec85E40f4cb697BAe';
        this.dashboardService.getDashboard().subscribe(model => {
            const db = model.json() as Dashboard;
            // db.clientInfoModel.balance = 4;
            // db.clientInfoModel.bonusBalance = 50;
            this.dashboard = db;
            // console.log(this.dashboard);

        });
       
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
@Component({
    selector: 'succsess-transfer-dialog',
    template: `<h4>Token transfer was made. You should see your DTT tokens in your ERC20 wallet within an hour!</h4>

    <button style="float: right" (click)="dialogRef.close()" mat-raised-button>
        <span>{{'buttons.Close' | translate}}</span>
    </button>`
})
export class SuccsessTransferDialogComponent {


    constructor(
        public dialogRef: MatDialogRef<SuccsessTransferDialogComponent>) {

    }

}

@Component({
    selector: 'failed-transfer-dialog',
    template: `<h4>Sorry, there were problems with tokens transfer.
    You may try again latter or contact support@data-trading.com for further help</h4>

    <button style="float: right" (click)="dialogRef.close()" mat-raised-button>
        <span>{{'buttons.Close' | translate}}</span>
    </button>`
})
export class FailedTransferDialogComponent {


    constructor(
        public dialogRef: MatDialogRef<FailedTransferDialogComponent>) {

    }

}
