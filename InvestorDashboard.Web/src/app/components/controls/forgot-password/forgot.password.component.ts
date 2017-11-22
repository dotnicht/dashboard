import { Component, OnInit, Inject } from '@angular/core';
import { ForgotPassWord } from '../../../models/user-edit.model';
import { AccountEndpoint } from '../../../services/account-endpoint.service';
import { Utilities } from '../../../services/utilities';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialog } from '@angular/material';
import { Router } from '@angular/router';
import { DOCUMENT } from '@angular/common';
import { BaseComponent } from '../../../base.component';

@Component({
    selector: 'app-forgot-password',
    templateUrl: './forgot.password.component.html',
    styleUrls: ['./forgot.password.component.scss']
})
/** restore-password component*/
export class ForgotPasswordComponent extends BaseComponent implements OnInit {

    forgotPasswordDialogRef: MatDialogRef<ForgotPasswordDialogComponent> | null;
    forgotPassword: ForgotPassWord = new ForgotPassWord();

    /** restore-password ctor */
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

    /** Called by Angular after restore-password component initialized */
    ngOnInit(): void { }

    openForgotPasswordDialog() {
        let config = {
            disableClose: true,
            hasBackdrop: false,
            panelClass: 'email-confirm-dialog',
            data: this.forgotPassword.email
        };

        this.forgotPasswordDialogRef = this.dialog.open(ForgotPasswordDialogComponent, config);
    }

    OnSubmit() {
        this.isLoading = true;
        this.errorMsg = '';
        this.accountEndpoint.forgotPasswordEndpoint(this.forgotPassword).subscribe(
            resp => {
                setTimeout(() => {
                    this.isLoading = false;
                    this.errorMsg = '';
                    this.openForgotPasswordDialog();
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
    selector: 'app-forgot-password-dialog',
    template: `
    <h2 mat-dialog-title><mat-icon>announcement</mat-icon> Please check your email to reset your password.
    <b>{{email}}</b></h2>
    <button style="float: right" (click)="close()" mat-raised-button tabindex="1">
        <span>{{'buttons.Close' | translate}}</span>
    </button>
    `
})
export class ForgotPasswordDialogComponent {

    email: string;
    constructor(public dialogRef: MatDialogRef<ForgotPasswordDialogComponent>,
        private router: Router,
        @Inject(MAT_DIALOG_DATA) public data: any) {

    }
    close() {
        this.dialogRef.close();
        this.router.navigate(['/login']);
    }

}