import { Component, Inject, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialog } from '@angular/material';
import { Router } from '@angular/router';
import { DOCUMENT } from '@angular/platform-browser';
import { AccountEndpoint } from '../../../services/account-endpoint.service';
import { BaseComponent } from '../../../base.component';
import { ForgotPassWord } from '../../../models/user-edit.model';
import { Utilities } from '../../../services/utilities';

export class ResendEmailConfirmCode extends ForgotPassWord {

}

@Component({
    selector: 'app-resend-email-confirm-code',
    templateUrl: './resend-email-confirm-code.component.html',
    styleUrls: ['./resend-email-confirm-code.component.scss']
})
/** ResendEmailConfirmCode component*/
export class ResendEmailConfirmCodeComponent extends BaseComponent {


    resendEmailConfirmCodeDialogRef: MatDialogRef<ResendEmailConfirmCodeDialogComponent> | null;
    resendEmailConfirmCode: ResendEmailConfirmCode = new ResendEmailConfirmCode();
    isLoading = false;
    errorMsg = '';
    constructor(private accountEndpoint: AccountEndpoint,
        private dialog: MatDialog,
        @Inject(DOCUMENT) doc: any) {
        super();
        dialog.afterOpen.subscribe(() => {
            if (!doc.body.classList.contains('no-scroll')) {
                doc.body.classList.add('no-scroll');
            }
        });
        dialog.afterAllClosed.subscribe(() => {
            doc.body.classList.remove('no-scroll');
        });

    }

    openEmailConfirmCodeDialog() {
        let config = {
            disableClose: true,
            hasBackdrop: false,
            data: this.resendEmailConfirmCode.email
        };

        this.resendEmailConfirmCodeDialogRef = this.dialog.open(ResendEmailConfirmCodeDialogComponent, config);

    }
    OnSubmit() {
        this.isLoading = true;
        this.errorMsg = '';
        this.accountEndpoint.resendEmailConfirmCodeEndpoint(this.resendEmailConfirmCode).subscribe(
            resp => {
                setTimeout(() => {
                    this.isLoading = false;
                    this.errorMsg = '';
                    this.openEmailConfirmCodeDialog();
                }, 2000);
            },
            error => {
                this.isLoading = false;

                const errorMessage = Utilities.findHttpResponseMessage('error_description', error);
                console.log(Utilities.findHttpResponseMessage('error', error));
                if (errorMessage) {
                    // this.alertService.showStickyMessage('Unable to login', errorMessage, MessageSeverity.error, error);
                    this.errorMsg = errorMessage;
                }
                else
                    this.errorMsg = 'An error occured whilst logging in, please try again later.\nError: ' + Utilities.findHttpResponseMessage('error', error);


            });
    }
}

@Component({
    selector: 'app-resend-email-code-dialog',
    template: `
        <p>Email with activation code was sent to
        	<b>{{email}}</b>
        </p>
        <button style="float: right" (click)="close()" mat-raised-button>
        	<span>{{'buttons.Close' | translate}}</span>
        </button>`
})
export class ResendEmailConfirmCodeDialogComponent {

    email: string;
    constructor(public dialogRef: MatDialogRef<ResendEmailConfirmCodeDialogComponent>,
        private router: Router,
        @Inject(MAT_DIALOG_DATA) public data: any) {
        this.email = data;

    }
    close() {
        this.dialogRef.close();
        this.router.navigate(['/login']);
    }

}