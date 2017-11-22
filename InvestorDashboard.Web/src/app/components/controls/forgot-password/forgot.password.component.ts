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

    resetPasswordDialogRef: MatDialogRef<ResetPasswordDialogComponent> | null;
    resetPassword: ForgotPassWord = new ForgotPassWord();

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

    openResetPasswordDialog() {
        let config = {
            disableClose: true,
            hasBackdrop: false,
            panelClass: 'email-confirm-dialog',
            data: this.resetPassword.email
        };

        this.resetPasswordDialogRef = this.dialog.open(ResetPasswordDialogComponent, config);
    }

    OnSubmit() {
        this.isLoading = true;
        this.errorMsg = '';
        this.accountEndpoint.forgotPasswordEndpoint(this.resetPassword).subscribe(
            resp => {
                setTimeout(() => {
                    this.isLoading = false;
                    this.errorMsg = '';
                    this.openResetPasswordDialog();
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
    selector: 'app-reset-password-dialog',
    template: `
    <p>
    Please check your email to reset your password.
        <b>{{email}}</b>
    </p>
    <button style="float: right" (click)="close()" mat-raised-button>
        <span>{{'buttons.Close' | translate}}</span>
    </button>
    `
})
export class ResetPasswordDialogComponent {

    email: string;
    constructor(public dialogRef: MatDialogRef<ResetPasswordDialogComponent>,
        private router: Router,
        @Inject(MAT_DIALOG_DATA) public data: any) {

    }
    close() {
        this.dialogRef.close();
        this.router.navigate(['/login']);
    }

}