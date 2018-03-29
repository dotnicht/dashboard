import { Component, Inject } from '@angular/core';
import { ReferralInfo } from '../../models/referral/referral-info.model';
import { ReferralService } from '../../services/referral.service';
import { ReferralCurrencyItem } from '../../models/referral/referral-currency-item.model';
import { MatDialogRef, MatDialog, MAT_DIALOG_DATA } from '@angular/material';
import { Router } from '@angular/router';
import { DOCUMENT } from '@angular/platform-browser';


@Component({
    selector: 'app-referral',
    templateUrl: './referral.component.html',
    styleUrls: ['./referral.component.scss']
})
/** referral component*/
export class ReferralComponent {
    /** referral ctor */
    confirmChangingAddressDialogRef: MatDialogRef<ConfirmChangingAddressDialogComponent> | null;
    
    config = {
        // disableClose: true,
        // hasBackdrop: false,
        // panelClass: 'register-rules-dialog',
        data: {},
    };

    public referralInfo: ReferralInfo;
    public CURRENCIES =
        {
            'BTC': 'Bitcoin',
            'ETH': 'Etherium'
        };

    public referralLinkIsCopied: boolean = false;

    constructor(private referralService: ReferralService,
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
    }

    ngOnInit() {
        this.referralService.getReferralInfo().subscribe((data: ReferralInfo) => {
            this.referralInfo = data;
            for (let item of this.referralInfo.items) {
                item.previousAddress = item.address;
                item.currName = this.CURRENCIES[item.currAcronym];
            }
        });

    }

    copyToClipboard(copiedElement: string) {
        if (copiedElement.toUpperCase() == 'REF_LINK') {
            this.referralLinkIsCopied = true;
        }
        else {
            this.referralLinkIsCopied = false;
        }

        for (let item of this.referralInfo.items) {
            if (item.currAcronym == copiedElement.toUpperCase()) {
                item.addressIsCopied = true;
            }
            else {
                item.addressIsCopied = false;
            }
        }
    }

    private save(currencyAcronym: string) {
        let item = this.referralInfo.items.find((item) => {
            if (item.currAcronym == currencyAcronym) {
                return true;
            }
            return false;
        });

        this.config.data['newAddress'] = item.address;
        this.config.data['previousAddress'] = item.previousAddress;
        this.config.data['remove'] = false;

        this.confirmChangingAddressDialogRef = this.dialog.open(ConfirmChangingAddressDialogComponent, this.config);
        this.confirmChangingAddressDialogRef.afterClosed().subscribe((result) => {
            if (result == true) {
                this.referralService.changeReferralInfo(currencyAcronym, item.address).subscribe();
                item.isEditModeRefAddress = false;
                item.readonlyRefAddress = !item.isEditModeRefAddress;
                this.config.data = {};
            }
        });
    }

    private edit(currencyAcronym: string) {
        currencyAcronym = currencyAcronym.toUpperCase();
        let item = this.referralInfo.items.find((item) => {
            if (item.currAcronym == currencyAcronym) {
                return true;
            }
            return false;
        });
        item.isEditModeRefAddress = true;
        item.readonlyRefAddress = !item.isEditModeRefAddress;
    }
    
    private cancel(currencyAcronym: string) {
        currencyAcronym = currencyAcronym.toUpperCase();
        let item = this.referralInfo.items.find((item) => {
            if (item.currAcronym == currencyAcronym) {
                return true;
            }
            return false;
        });

        item.isEditModeRefAddress = false;
        item.readonlyRefAddress = !item.isEditModeRefAddress;
        item.address = item.previousAddress;
    }

    private delete(currencyAcronym: string) {
        currencyAcronym = currencyAcronym.toUpperCase();
        let item = this.referralInfo.items.find((item) => {
            if (item.currAcronym == currencyAcronym) {
                return true;
            }
            return false;
        });

        this.config.data['newAddress'] = item.address;
        this.config.data['remove'] = true;
        
        this.confirmChangingAddressDialogRef = this.dialog.open(ConfirmChangingAddressDialogComponent, this.config);
        this.confirmChangingAddressDialogRef.afterClosed().subscribe((result) => {
            if (result == true) {
                this.referralService.changeReferralInfo(currencyAcronym, item.address).subscribe();
                item.isEditModeRefAddress = false;
                item.readonlyRefAddress = !item.isEditModeRefAddress;
                item.address = null;
                this.config.data = {};
            }
        });

        this.referralService.changeReferralInfo(currencyAcronym).subscribe();
    }
}


@Component({
    selector: 'confirm-changing-address-dialog',
    templateUrl: './confirm-changing-address-dialog.component.html'
})
export class ConfirmChangingAddressDialogComponent {
    public info: any;

    constructor(
        public dialogRef: MatDialogRef<ConfirmChangingAddressDialogComponent>,
        private router: Router,
        @Inject(MAT_DIALOG_DATA) public data: any) {
        this.info = data;
    }

    close() {
        this.dialogRef.close();
        // document.location.href = '/login';
    }
}

