import { Component, OnInit, OnDestroy, Input } from '@angular/core';
import { UserLogin, User } from '../../models/user.model';
import { AuthService } from '../../services/auth.service';
import { ConfigurationService } from '../../services/configuration.service';
import { Utilities } from '../../services/utilities';
import { Router } from '@angular/router';
import { ReferralService } from '../../services/referral.service';

@Component({
    selector: 'app-login',
    templateUrl: './login.component.html',
    styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit, OnDestroy {

    userLogin = new UserLogin();
    isLoading = false;
    formResetToggle = true;
    loginStatusSubscription: any;
    errorMsg: string;
    showConfirmEmailLink = false;
    queryParams: any = null;

    @Input() isModal = false;

    constructor(private router: Router,
        private authService: AuthService,
        private configurations: ConfigurationService,
        private referralService: ReferralService
    ) { }

    ngOnInit(): void {
        if (Object.keys(this.referralService.queryParams).length != 0) {
            this.queryParams = this.referralService.queryParams;
        }

        if (this.authService.isLoggedIn) {
            if (Object.keys(this.referralService.queryParams).length != 0) {
                this.router.navigate(['/login'], { queryParams: this.referralService.queryParams });
            } else {
                this.router.navigate(['/login']);
            }
        }

        this.loginStatusSubscription = this.authService.state$.subscribe(data => {
            if (data.tokens != null) {
                if (data.authReady) {
                    this.router.navigate(['/']);
                }
            }
        });

    }
    ngOnDestroy() {
        if (this.loginStatusSubscription) {
            this.loginStatusSubscription.unsubscribe();
        }
    }


    login() {
        this.isLoading = true;
        this.errorMsg = '';
        this.authService.login(this.userLogin)
            .subscribe(
                user => {
                    const _user = user as User;

                    setTimeout(() => {
                        //this.alertService.stopLoadingMessage();
                        this.isLoading = false;
                        this.reset();
                        // if (_user.twoFactorEnabled) {
                        //     this.router.navigate(['/tfa']);
                        // }
                        //this.alertService.showMessage('Login', `Welcome ${user.userName}!`, MessageSeverity.success);
                    }, 500);
                },
                error => {
                    this.isLoading = false;
                    // this.alertService.stopLoadingMessage();

                    if (Utilities.checkNoNetwork(error)) {
                        //this.alertService.showStickyMessage(Utilities.noNetworkMessageCaption, Utilities.noNetworkMessageDetail, MessageSeverity.error, error);
                    }
                    else {
                        let errorMessage = Utilities.findHttpResponseMessage('error_description', error);


                        const errorType = Utilities.findHttpResponseMessage('error', error);
                        console.log(errorType);
                        if (errorType == 'confirm_email') {
                            this.showConfirmEmailLink = true;
                        }
                        if (errorMessage) {
                            // this.alertService.showStickyMessage('Unable to login', errorMessage, MessageSeverity.error, error);
                            this.errorMsg = errorMessage;
                        } else {
                            this.errorMsg = 'An error occured whilst logging in, please try again later.\nError: ' + error.statusText || error.status;
                        }

                    }

                });
    }

    reset() {
        this.formResetToggle = false;

        setTimeout(() => {
            this.formResetToggle = true;
        });
    }
    onFocus() {
        this.errorMsg = '';
    }

}
