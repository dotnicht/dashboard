import { Component, OnInit, Inject } from '@angular/core';
import { ChangePassWord, ResetPassword } from '../../../models/user-edit.model';
import { ActivatedRoute, Params, Router } from '@angular/router';
import { CookieService } from 'ngx-cookie-service';
import { AccountEndpoint } from '../../../services/account-endpoint.service';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';
import { DOCUMENT } from '@angular/platform-browser';
import { Utilities } from '../../../services/utilities';

@Component({
    selector: 'app-reset-password',
    templateUrl: './reset-password.component.html',
    styleUrls: ['./reset-password.component.scss']
})
/** reset-password component*/
export class ResetPasswordComponent {
    resetPasswordDialogRef: MatDialogRef<ResetPasswordDialogComponent> | null;
    public resetPassWordForm = new ResetPassword();
    isLoading = false;
    errorMsg: string;
    constructor(private cookieService: CookieService,
        private accountEndpoint: AccountEndpoint,
        private dialog: MatDialog,
        @Inject(DOCUMENT) doc: any) {

    }
    openResetPasswordDialog() {
        let config = {
            disableClose: true,
            hasBackdrop: true,
            panelClass: 'reset-password-dialog'
        };

        this.resetPasswordDialogRef = this.dialog.open(ResetPasswordDialogComponent, config);
    }
    OnSubmit() {
        this.resetPassWordForm.code = this.cookieService.get('reset_token');
        this.resetPassWordForm.email = this.cookieService.get('reset_email');
        this.isLoading = true;
        this.errorMsg = '';

        this.accountEndpoint.resetPasswordEndpoint(this.resetPassWordForm).subscribe(
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

    <h2 mat-dialog-title><mat-icon>check_circle</mat-icon>Password successfully changed</h2>
        <button style="float: right" (click)="close()" mat-raised-button tabindex="1">
            <span>{{'buttons.Close' | translate}}</span>
        </button>
    `
})
export class ResetPasswordDialogComponent {

    email: string;
    constructor(public dialogRef: MatDialogRef<ResetPasswordDialogComponent>,
        private router: Router) {

    }
    close() {
        this.dialogRef.close();
        this.router.navigate(['/login']);
    }

}