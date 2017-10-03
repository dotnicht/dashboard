import { Component, OnInit, OnDestroy, Input } from '@angular/core';
import { UserLogin } from '../../models/user.model';
import { AuthService } from '../../services/auth.service';
import { AlertService, MessageSeverity, DialogType } from '../../services/alert.service';
import { ConfigurationService } from '../../services/configuration.service';
import { Utilities } from '../../services/utilities';
import { MdDialog } from '@angular/material';

@Component({
    selector: 'app-login',
    templateUrl: './login.component.html',
    styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit, OnDestroy {

    userLogin = new UserLogin();
    isLoading = false;
    formResetToggle = true;
    modalClosedCallback: () => void;
    loginStatusSubscription: any;

    dialogConfig = {
        disableClose: false,
        panelClass: 'custom-overlay-pane-class',
        hasBackdrop: true,
        backdropClass: '',
        width: '',
        height: '',
        position: {
          top: '',
          bottom: '',
          left: '',
          right: ''
        },
        data: {
          message: 'Jazzy jazz jazz'
        }
      };

    @Input() isModal = false;

    constructor(private alertService: AlertService, private authService: AuthService, private configurations: ConfigurationService, private dialog: MdDialog) { }

    ngOnInit(): void {
        this.userLogin.rememberMe = this.authService.rememberMe;

        this.userLogin.email = 'denis.skvortsow@gmail.com';
        this.userLogin.password = '123456_Kol';
    }
    ngOnDestroy() {
        if (this.loginStatusSubscription)
            this.loginStatusSubscription.unsubscribe();
    }


    getShouldRedirect() {
        return !this.isModal && this.authService.isLoggedIn && !this.authService.isSessionExpired;
    }


    showErrorAlert(caption: string, message: string) {
        this.alertService.showMessage(caption, message, MessageSeverity.error);
    }

    closeModal() {
        if (this.modalClosedCallback) {
            this.modalClosedCallback();
        }
    }
    login() {
        this.authService.login(this.userLogin)
            .subscribe(
            user => {
                setTimeout(() => {
                    this.alertService.stopLoadingMessage();
                    this.isLoading = false;
                    this.reset();

                    if (!this.isModal) {
                        this.alertService.showMessage('Login', `Welcome ${user.userName}!`, MessageSeverity.success);

                        let dialogRef = this.dialog.open(TestDialog, this.dialogConfig);
                    }
                    else {
                        let dialogRef = this.dialog.open(TestDialog, this.dialogConfig);
                        
                        this.alertService.showMessage('Login', `Session for ${user.userName} restored!`, MessageSeverity.success);
                        setTimeout(() => {
                            this.alertService.showStickyMessage('Session Restored', 'Please try your last operation again', MessageSeverity.default);
                        }, 500);

                        this.closeModal();
                    }
                }, 500);
            },
            error => {

                this.alertService.stopLoadingMessage();

                if (Utilities.checkNoNetwork(error)) {
                    this.alertService.showStickyMessage(Utilities.noNetworkMessageCaption, Utilities.noNetworkMessageDetail, MessageSeverity.error, error);
                    this.offerAlternateHost();
                }
                else {
                    let errorMessage = Utilities.findHttpResponseMessage('error_description', error);

                    if (errorMessage)
                        this.alertService.showStickyMessage('Unable to login', errorMessage, MessageSeverity.error, error);
                    else
                        this.alertService.showStickyMessage('Unable to login', 'An error occured whilst logging in, please try again later.\nError: ' + error.statusText || error.status, MessageSeverity.error, error);
                }

                setTimeout(() => {
                    this.isLoading = false;
                }, 500);
            });
    }
    offerAlternateHost() {

        if (Utilities.checkIsLocalHost(location.origin) && Utilities.checkIsLocalHost(this.configurations.baseUrl)) {
            this.alertService.showDialog('Dear Developer!\nIt appears your backend Web API service is not running...\n' +
                'Would you want to temporarily switch to the online Demo API below?(Or specify another)',
                DialogType.prompt,
                (value: string) => {
                    this.configurations.baseUrl = value;
                    this.alertService.showStickyMessage('API Changed!', 'The target Web API has been changed to: ' + value, MessageSeverity.warn);
                },
                null,
                null,
                null,
                this.configurations.fallbackBaseUrl);
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
    selector: 'demo-iframe-dialog',
    styles: [
        `iframe {
        width: 100px;
      }`
    ],
    template: `
      <h2 md-dialog-title>Success</h2>
  
      <md-dialog-content>
        Success login!
      </md-dialog-content>
    `
})
export class TestDialog {

}