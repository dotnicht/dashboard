import { Component, OnInit, Inject } from '@angular/core';
import { FormControl, Validators } from '@angular/forms';
import { UserRegister, RegisterRules } from '../../models/user.model';
import { Http } from '@angular/http';
import { AuthService } from '../../services/auth.service';
import { AlertService, MessageSeverity } from '../../services/alert.service';
import { Utilities } from '../../services/utilities';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialog, MatDialogConfig } from '@angular/material';
import { DOCUMENT } from '@angular/platform-browser';
import { AppTranslationService } from '../../services/app-translation.service';

const defaultDialogConfig = new MatDialogConfig();

@Component({
    selector: 'app-register',
    templateUrl: './register.component.html',
    styleUrls: ['./register.component.scss']
})
export class RegisterComponent implements OnInit {

    registerRulesDialogRef: MatDialogRef<RegisterRulesDialogComponent> | null;
    emailConfirmDialogRef: MatDialogRef<ConfirmEmailDialogComponent> | null;

    registerRules: RegisterRules[] = [
        { name: this.translationService.getTranslation('users.register.rules.first'), checked: false },
        { name: this.translationService.getTranslation('users.register.rules.second'), checked: false },
        // { name: this.translationService.getTranslation('users.register.rules.third'), checked: false },
        { name: this.translationService.getTranslation('users.register.rules.fourth'), checked: false }
    ];
    formResetToggle = true;
    isLoading = false;
    registerForm = new UserRegister();


    config = {
        disableClose: true,
        hasBackdrop: false,
        panelClass: 'register-rules-dialog',
        data: this.registerRules
    };

    constructor(private translationService: AppTranslationService, private alertService: AlertService, private authService: AuthService,
        public dialog: MatDialog, @Inject(DOCUMENT) doc: any) {
        dialog.afterOpen.subscribe(() => {
            if (!doc.body.classList.contains('no-scroll')) {
                doc.body.classList.add('no-scroll');
            }
        });
        dialog.afterAllClosed.subscribe(() => {
            doc.body.classList.remove('no-scroll');
        });

    }


    /** Called by Angular after register component initialized */
    ngOnInit(): void {
        // if (this.authService.isLoggedIn) {

        // }
        //this.registerForm.email = 'denis.skvortsow@gmail.com';
        //this.registerForm.password = '123456_Kol';
        //this.registerForm.confirmPassword = '123456_Kol';



    }
    openEmailConfirmDialog(): void {
        let config = {
            disableClose: true,
            hasBackdrop: false,
            panelClass: 'email-confirm-dialog',
            data: this.registerForm.email
        };

        this.emailConfirmDialogRef = this.dialog.open(ConfirmEmailDialogComponent, config);
    }

    openRegisterRulesDialog(): void {
        this.registerRulesDialogRef = this.dialog.open(RegisterRulesDialogComponent, this.config);
        this.registerRulesDialogRef.afterClosed().subscribe((result: RegisterRules[]) => {
            this.isLoading = true;
            this.registerRules = result;
            this.registerRulesDialogRef = undefined;

            this.registerRules.forEach(element => {
                element.checked = false;
            });

            this.alertService.startLoadingMessage();
            this.authService.register(this.registerForm).subscribe(responce => {
                setTimeout(() => {
                    this.alertService.stopLoadingMessage();
                    this.isLoading = false;
                    this.reset();
                    this.openEmailConfirmDialog();
                    this.alertService.showMessage('Register', `Successful registration!`, MessageSeverity.success);
                    this.alertService.showMessage('Message Has Been Sent', `Link to complete registration has been sent to` + this.registerForm.email, MessageSeverity.info);
                }, 1000);
            },
                error => {

                    this.alertService.stopLoadingMessage();

                    if (Utilities.checkNoNetwork(error)) {
                        this.alertService.showStickyMessage(Utilities.noNetworkMessageCaption, Utilities.noNetworkMessageDetail, MessageSeverity.error, error);
                    }
                    else {
                        let errorType = Utilities.findHttpResponseMessage('error', error);
                        let errorMessage = Utilities.findHttpResponseMessage('error_description', error);

                        if (errorType == 'user_exist') {
                            this.alertService.showStickyMessage('Unable to register', 'User already exists!', MessageSeverity.error, error);
                        }
                        else if (errorMessage)
                            this.alertService.showStickyMessage('Unable to register', errorMessage, MessageSeverity.error, error);
                        else
                            this.alertService.showStickyMessage('Unable to register', 'An error occured whilst logging in, please try again later.\nError: ' + error.statusText || error.status, MessageSeverity.error, error);
                    }

                    setTimeout(() => {
                        this.isLoading = false;
                    }, 1000);
                });
        });
    }
    OnSubmit() {
        if (this.registerRules !== undefined && this.registerRules.every((element, index, array) => {
            return element.checked;
        })) {

        } else {
            this.openRegisterRulesDialog();
        }


    }

    reset() {
        this.formResetToggle = false;

        setTimeout(() => {
            this.formResetToggle = true;
        });
    }

}


@Component({
    selector: 'register-rules-dialog',
    templateUrl: './register-rules.dialog.component.html'
})
export class RegisterRulesDialogComponent {
    public _acceptRules: RegisterRules[];

    constructor(
        public dialogRef: MatDialogRef<RegisterRulesDialogComponent>,

        @Inject(MAT_DIALOG_DATA) public data: any) {
        this._acceptRules = data;
    }

    get acceptAllRules() {
        return this._acceptRules.every((element, index, array) => { return element.checked; });
    }
}
@Component({
    selector: 'confirm-email-dialog',
    templateUrl: './confirm-email.dialog.component.html'
})
export class ConfirmEmailDialogComponent {
    public email: string;

    constructor(
        public dialogRef: MatDialogRef<ConfirmEmailDialogComponent>,

        @Inject(MAT_DIALOG_DATA) public data: any) {
        this.email = data;
    }

}