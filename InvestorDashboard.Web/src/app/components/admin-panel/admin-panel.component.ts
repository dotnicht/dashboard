import { Component, Inject } from '@angular/core';
import { MatSlideToggleChange, MatDialogRef, MAT_DIALOG_DATA, MatDialog } from '@angular/material';
import { DOCUMENT } from '@angular/platform-browser';
import { AdminPanelService } from '../../services/admin-panel.service';

@Component({
    selector: 'app-admin-panel',
    templateUrl: './admin-panel.component.html',
    styleUrls: ['./admin-panel.component.scss']
})
/** adminPanel component*/
export class AdminPanelComponent {
    /** adminPanel ctor */
    confirmExtraTokensDialogRef: MatDialogRef<ConfirmExtraTokensDialogComponent> | null;
    email: string;
    extraTokens: number;
    private userGuid: string;
    userTransactions: any[];
    showUserTransactions: boolean;

    constructor(private dialog: MatDialog,
        private adminService: AdminPanelService,
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

    findEmail() {
        //dd.aa.nn.1.kk@gmail.com
        console.log('email', this.email);
        this.showUserTransactions = null;
        if (this.email && this.email.length > 0) {
            this.adminService.getUserTransactionsByEmail(this.email).subscribe(resp=> {
                let response = resp.json();
                if ('id' in response && 'transactions' in response) {
                    this.userGuid = response.id;
                    this.userTransactions = response.transactions;
                    this.showUserTransactions = true;
                }
            })
        }      
    }

    enterTokens() {
        let config = {
            data: {kek: this.extraTokens}
        }
        this.confirmExtraTokensDialogRef = this.dialog.open(ConfirmExtraTokensDialogComponent, config);
        this.confirmExtraTokensDialogRef.afterClosed().subscribe((result) => {
            if (result == true) {
                this.adminService.setTokensToUser(this.userGuid, this.extraTokens).subscribe(resp=> {
                    console.log("RESP", resp)
                });
                console.log(1123)
                // this.referralService.changeReferralInfo(currencyAcronym, item.address).subscribe();
                // item.isEditModeRefAddress = false;
                // item.readonlyRefAddress = !item.isEditModeRefAddress;

            }
        });
        console.log('tokens', this.extraTokens);
    }

    toggleTokens(event: MatSlideToggleChange) {
        let isATPToggled = event.checked ? false : true;
        console.log(isATPToggled)
    }
}


@Component({
    selector: 'confirm-extra-tokens-dialog',
    templateUrl: './confirm-extra-tokens-dialog.component.html'
})
export class ConfirmExtraTokensDialogComponent {
    public info: any;

    constructor(
        public dialogRef: MatDialogRef<ConfirmExtraTokensDialogComponent>,
        @Inject(MAT_DIALOG_DATA) public data: any
    ) {
        this.info = data;
    }

    close() {
        this.dialogRef.close();
    }
}