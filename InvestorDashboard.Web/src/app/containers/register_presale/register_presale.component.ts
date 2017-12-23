import { Component, ViewChild, Inject, OnInit } from '@angular/core';
import { OtherService } from '../../services/other.service';
import { ReCaptchaComponent } from 'angular2-recaptcha';
import { RegisterRules, UserRegister } from '../../models/user.model';
import { MatDialogRef, MatDialog } from '@angular/material';
import { RegisterRulesDialogComponent, ConfirmEmailDialogComponent } from '../../components/register/register.component';
import { Router } from '@angular/router';
import { AppTranslationService } from '../../services/app-translation.service';
import { AuthService } from '../../services/auth.service';
import { CookieService } from 'ngx-cookie-service';
import { DOCUMENT } from '@angular/common';
import { Utilities } from '../../services/utilities';

@Component({
    selector: 'app-register_presale',
    templateUrl: './register_presale.component.html',
    styleUrls: ['./register_presale.component.scss']
})
/** register_presale component*/
export class RegisterPreSaleComponent implements OnInit {
    /** register_presale ctor */

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
        private otherService: OtherService,
        @Inject(DOCUMENT) doc: any) {

        this.otherService.showMainComponent = false;
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

                if (this.cookieService.get('clickid') != '') {
                    this.registerForm.clickId = this.cookieService.get('clickid');
                }
                this.registerForm.reCaptchaToken = this.captcha.getResponse();
                console.log(this.registerForm);
                // this.alertService.startLoadingMessage();
                this.authService.register(this.registerForm).subscribe(responce => {
                    setTimeout(() => {

                        this.openEmailConfirmDialog();
                        this.reset();
                    }, 1000);
                },
                    error => {

                        // this.alertService.stopLoadingMessage();

                        this.reset();

                        let errorType = Utilities.findHttpResponseMessage('error', error);
                        let errorMessage = Utilities.findHttpResponseMessage('error_description', error);

                        if (errorType == 'user_exist') {
                            this.errorMsg = 'User already exists!';
                        }
                        else if (errorMessage) {
                            this.errorMsg = errorMessage;
                        }
                        else
                            this.errorMsg = 'An error occured whilst logging in, please try again later.\nError: ' + error.statusText || error.status;



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
