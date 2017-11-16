import { Component, OnInit, Inject, OnDestroy, ViewChild } from '@angular/core';
import { FormControl, Validators } from '@angular/forms';
import { UserRegister, RegisterRules } from '../../models/user.model';
import { Http } from '@angular/http';
import { AuthService } from '../../services/auth.service';
import { Utilities } from '../../services/utilities';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialog, MatDialogConfig } from '@angular/material';
import { DOCUMENT } from '@angular/platform-browser';
import { AppTranslationService } from '../../services/app-translation.service';
import { Router, ActivatedRouteSnapshot } from '@angular/router';
import { Observable } from 'rxjs/Observable';
import { CookieService } from 'ngx-cookie-service';
import { ReCaptchaComponent } from 'angular2-recaptcha';

const defaultDialogConfig = new MatDialogConfig();

@Component({
    selector: 'app-register',
    templateUrl: './register.component.html',
    styleUrls: ['./register.component.scss']
})
export class RegisterComponent implements OnInit {


    @ViewChild(ReCaptchaComponent) captcha: ReCaptchaComponent;
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
    errorMsg: string;
    registerForm = new UserRegister();
    reCaptchaStatus = false;


    config = {
        disableClose: true,
        hasBackdrop: false,
        panelClass: 'register-rules-dialog',
        data: this.registerRules
    };

    constructor(private router: Router,
        private translationService: AppTranslationService,
        private authService: AuthService,
        private cookieService: CookieService,
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


    /** Called by Angular after register component initialized */
    ngOnInit(): void {
        if (this.authService.isLoggedIn) {
            this.router.navigate(['/login']);
        }
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

    handleCorrectCaptcha(event) {
        this.reCaptchaStatus = true;
    }
    handleCaptchaExpired() {
        this.reCaptchaStatus = false;
    }

    openRegisterRulesDialog(): void {
        this.registerRulesDialogRef = this.dialog.open(RegisterRulesDialogComponent, this.config);
        this.registerRulesDialogRef.afterClosed().subscribe((result: RegisterRules[]) => {
            if (this.registerRules !== undefined && this.registerRules.every((element, index, array) => {
                return element.checked;
            })) {
                this.errorMsg = '';
                this.isLoading = true;
                this.registerRules = result;
                this.registerRulesDialogRef = undefined;

                this.registerRules.forEach(element => {
                    element.checked = false;
                });
                let registrationRequest = '';

                if (this.cookieService.get('clickid') != '') {
                    registrationRequest += `clickid=${this.cookieService.get('clickid')};`;
                }
                this.registerForm.registrationRequest = registrationRequest;
                // this.alertService.startLoadingMessage();
                this.authService.register(this.registerForm).subscribe(responce => {
                    setTimeout(() => {
                        // this.alertService.stopLoadingMessage();

                        this.reset();
                        this.openEmailConfirmDialog();
                        //this.alertService.showMessage('Register', `Successful registration!`, MessageSeverity.success);
                        //this.alertService.showMessage('Message Has Been Sent', `Link to complete registration has been sent to` + this.registerForm.email, MessageSeverity.info);
                    }, 1000);
                },
                    error => {

                        // this.alertService.stopLoadingMessage();

                        this.reset();
                        if (Utilities.checkNoNetwork(error)) {
                            //this.alertService.showStickyMessage(Utilities.noNetworkMessageCaption, Utilities.noNetworkMessageDetail, MessageSeverity.error, error);
                        }
                        else {
                            let errorType = Utilities.findHttpResponseMessage('error', error);
                            let errorMessage = Utilities.findHttpResponseMessage('error_description', error);

                            if (errorType == 'user_exist') {
                                // this.alertService.showStickyMessage('Unable to register', 'User already exists!', MessageSeverity.error, error);
                                this.errorMsg = 'User already exists!';
                            }
                            else if (errorMessage) {
                                // this.alertService.showStickyMessage('Unable to register', errorMessage, MessageSeverity.error, error);
                                this.errorMsg = errorMessage;
                            }
                            else
                                this.errorMsg = 'An error occured whilst logging in, please try again later.\nError: ' + error.statusText || error.status;
                            //this.alertService.showStickyMessage('Unable to register', 'An error occured whilst logging in, please try again later.\nError: ' + error.statusText || error.status, MessageSeverity.error, error);
                        }


                    });
            }
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
        this.captcha.reset();
        this.reCaptchaStatus = false;
        this.registerForm = new UserRegister();
        this.isLoading = false;

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

@Component({
    selector: 'confirmed-email',
    templateUrl: './confirmed-email.component.html'
})
export class ConfirmedEmailComponent implements OnInit, OnDestroy {
    public timer: number = 5;
    public progress: number = 0;

    private timerA;
    private timerB;

    ngOnInit(): void {

    }

    ngOnDestroy() {
        if (this.timerA) {
            clearInterval(this.timerA);
        } if (this.timerB) {
            clearInterval(this.timerB);
        }
    }

    constructor(private router: Router) {
        this.timerA = setInterval(() => {
            if (this.timer >= 1) {
                this.timer -= 1;
            } else {
                clearInterval(this.timerA);
                this.router.navigate(['/login']);
            }
        }, 1000);
        this.timerB = setInterval(() => {
            if (this.progress <= 100) {
                this.progress += 10;
            } else {


            }
        }, 500);
    }


}