import { Component, OnInit, OnDestroy, Input } from '@angular/core';
import { UserLogin } from '../../models/user.model';
import { AuthService } from '../../services/auth.service';
import { AlertService, MessageSeverity, DialogType } from '../../services/alert.service';
import { ConfigurationService } from '../../services/configuration.service';
import { Utilities } from '../../services/utilities';

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


    @Input() isModal = false;

    constructor(private alertService: AlertService, private authService: AuthService, private configurations: ConfigurationService) { }

    ngOnInit(): void {
        this.userLogin.rememberMe = this.authService.rememberMe;

        if (this.getShouldRedirect()) {
            this.authService.redirectLoginUser();
        }
        else {
            this.loginStatusSubscription = this.authService.getLoginStatusEvent().subscribe(isLoggedIn => {
                if (this.getShouldRedirect()) {
                    this.authService.redirectLoginUser();
                }
            });
        }

        //this.userLogin.email = 'denis.skvortsow@gmail.com';
        //this.userLogin.password = '123456_Kol';


    }
    ngOnDestroy() {
        if (this.loginStatusSubscription)
            this.loginStatusSubscription.unsubscribe();
    }


    getShouldRedirect() {
        return !this.isModal && this.authService.isLoggedIn && !this.authService.isSessionExpired;
    }

    login() {
        this.isLoading = true;

        this.authService.login(this.userLogin)
            .subscribe(
            user => {
                setTimeout(() => {
                    this.alertService.stopLoadingMessage();
                    this.isLoading = false;
                    this.reset();

                    this.alertService.showMessage('Login', `Welcome ${user.userName}!`, MessageSeverity.success);
                }, 500);
            },
            error => {
                this.isLoading = false;
                this.alertService.stopLoadingMessage();

                if (Utilities.checkNoNetwork(error)) {
                    this.alertService.showStickyMessage(Utilities.noNetworkMessageCaption, Utilities.noNetworkMessageDetail, MessageSeverity.error, error);
                }
                else {
                    let errorMessage = Utilities.findHttpResponseMessage('error_description', error);

                    if (errorMessage) {
                        // this.alertService.showStickyMessage('Unable to login', errorMessage, MessageSeverity.error, error);
                        this.errorMsg = errorMessage;
                    }
                    else
                        this.errorMsg = 'An error occured whilst logging in, please try again later.\nError: ' + error.statusText || error.status;
                    // this.alertService.showStickyMessage('Unable to login', 'An error occured whilst logging in, please try again later.\nError: ' + error.statusText || error.status, MessageSeverity.error, error);
                }

            });
    }

    reset() {
        this.formResetToggle = false;

        setTimeout(() => {
            this.formResetToggle = true;
        });
    }


}
