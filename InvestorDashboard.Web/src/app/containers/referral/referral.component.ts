import { Component } from '@angular/core';
import { ReferralInfo } from '../../models/referral/referral-info.model';
import { ReferralService } from '../../services/referral.service';
import { ReferralCurrencyItem } from '../../models/referral/referral-currency-item.model';
import { MatDialogRef, MatDialog, MAT_DIALOG_DATA } from '@angular/material';
// import { Router } from '@angular/router';
// import { DOCUMENT } from '@angular/platform-browser';


@Component({
    selector: 'app-referral',
    templateUrl: './referral.component.html',
    styleUrls: ['./referral.component.scss']
})
/** referral component*/
export class ReferralComponent {
    /** referral ctor */

    public referralInfo: ReferralInfo;
    
    //todo refactor currencies to config file or smth like this. This list of currencies also in referral component.
    public CURRENCIES = [
        { acronym: 'BTC', name: 'Bitcoin' },
        { acronym: 'ETH', name: 'Etherium' }
    ];

    public referralLinkIsCopied: boolean = false;

    constructor(private referralService: ReferralService,
        private dialog: MatDialog) {
        // @Inject(DOCUMENT) doc: any) {

        //     dialog.afterOpen.subscribe(() => {
        //         if (!doc.body.classList.contains('no-scroll')) {
        //             doc.body.classList.add('no-scroll');
        //         }
        //     });
        //     dialog.afterAllClosed.subscribe(() => {
        //         doc.body.classList.remove('no-scroll');
        //     });
    }

    ngOnInit() {
        this.referralService.getReferralInfo().subscribe((data: ReferralInfo) => {
            this.referralInfo = data;
            for (let curr of this.CURRENCIES) {
                this.referralInfo.items[curr.acronym].previousAddress = this.referralInfo.items[curr.acronym].address;
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

        for (let curr of this.CURRENCIES) {
            if (curr.acronym == copiedElement.toUpperCase()) {
                this.referralInfo.items[curr.acronym].addressIsCopied = true;
            }
            else {
                this.referralInfo.items[curr.acronym].addressIsCopied = false;
            }
        }
    }

    private save(currencyAcronym: string) {
        this.referralService.changeReferralInfo(currencyAcronym, this.referralInfo.items[currencyAcronym].address).subscribe();
    }

    private edit(currencyAcronym: string) {
        currencyAcronym = currencyAcronym.toUpperCase();
        this.referralInfo.items[currencyAcronym].isEditModeRefAddress = true;
        this.referralInfo.items[currencyAcronym].readonlyRefAddress = !this.referralInfo.items[currencyAcronym].isEditModeRefAddress;
    }

    private cancel(currencyAcronym: string) {
        currencyAcronym = currencyAcronym.toUpperCase();
        this.referralInfo.items[currencyAcronym].isEditModeRefAddress = false;
        this.referralInfo.items[currencyAcronym].readonlyRefAddress = !this.referralInfo.items[currencyAcronym].isEditModeRefAddress;
        this.referralInfo.items[currencyAcronym].address = this.referralInfo.items[currencyAcronym].previousAddress;
    }

    private delete(currencyAcronym: string) {
        this.referralService.changeReferralInfo(currencyAcronym).subscribe();
    }
}


// @Component({
//     selector: 'confirm-email-dialog',
//     templateUrl: './confirm-email.dialog.component.html'
// })
// export class ConfirmEmailDialogComponent {
//     public email: string;

//     constructor(
//         public dialogRef: MatDialogRef<ConfirmEmailDialogComponent>,
//         private router: Router,
//         @Inject(MAT_DIALOG_DATA) public data: any) {
//         this.email = data;
//     }

//     close() {
//         this.dialogRef.close();
//         document.location.href = '/login';
//     }
// }

