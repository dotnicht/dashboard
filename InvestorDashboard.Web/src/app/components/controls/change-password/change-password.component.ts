import { Component, OnInit, Inject, ViewChild, ViewChildren } from '@angular/core';
import { ChangePassWord } from '../../../models/user-edit.model';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialog } from '@angular/material';
import { Router } from '@angular/router';
import { BaseComponent } from '../../../base.component';
import { AccountEndpoint } from '../../../services/account-endpoint.service';
import { DOCUMENT } from '@angular/common';
import { Utilities } from '../../../services/utilities';
import { EqualValidator } from '../../../directives/equal-validator.directive';

@Component({
    selector: 'app-change-password',
    templateUrl: './change-password.component.html',
    styleUrls: ['./change-password.component.scss']
})
/** change-password component*/
export class ChangePasswordComponent extends BaseComponent {
    /** change-password ctor */
    changePasswordDialogRef: MatDialogRef<ChangePasswordDialogComponent> | null;
    public changePassWordForm = new ChangePassWord();
    isLoading = false;
    errorMsg: string;

    /** restore-password ctor */
    constructor(
        private accountEndpoint: AccountEndpoint,
        private dialog: MatDialog,
        @Inject(DOCUMENT) doc: any
    ) {
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
    openChangePasswordDialog() {
        const config = {
            disableClose: true,
            hasBackdrop: false,
            panelClass: 'change-password-dialog'
        };

        this.changePasswordDialogRef = this.dialog.open(ChangePasswordDialogComponent, config);
    }
    OnSubmit() {
        this.isLoading = true;
        this.errorMsg = '';
        this.accountEndpoint.changePasswordEndpoint(this.changePassWordForm).subscribe(
            resp => {
                setTimeout(() => {
                    this.isLoading = false;
                    this.errorMsg = '';
                    this.openChangePasswordDialog();
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
    selector: 'app-change-password-dialog',
    template: `

    <h2 mat-dialog-title><mat-icon>check_circle</mat-icon>Password successfully changed</h2>
        <button style="float: right" (click)="close()" mat-raised-button tabindex="1">
            <span>{{'buttons.Close' | translate}}</span>
        </button>
    `
})
export class ChangePasswordDialogComponent {

    email: string;
    constructor(public dialogRef: MatDialogRef<ChangePasswordDialogComponent>,
        private router: Router,
        @Inject(MAT_DIALOG_DATA) public data: any) {

    }
    close() {
        this.dialogRef.close();
    }

}
