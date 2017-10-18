import { Component, OnInit } from '@angular/core';
import { FormControl, Validators } from '@angular/forms';
import { UserRegister } from '../../models/user.model';
import { Http } from '@angular/http';
import { AuthService } from '../../services/auth.service';
import { AlertService, MessageSeverity } from '../../services/alert.service';
import { Utilities } from '../../services/utilities';

@Component({
    selector: 'app-register',
    templateUrl: './register.component.html',
    styleUrls: ['./register.component.scss']
})

export class RegisterComponent implements OnInit {
    formResetToggle = true;
    isLoading = false;
    registerForm = new UserRegister();

    constructor(private alertService: AlertService, private authService: AuthService) {

    }


    /** Called by Angular after register component initialized */
    ngOnInit(): void {
        this.registerForm.email = 'denis.skvortsow@gmail.com';
        this.registerForm.password = '123456_Kol';
        this.registerForm.confirmPassword = '123456_Kol';
    }

    OnSubmit() {
        this.alertService.startLoadingMessage();
        this.authService.register(this.registerForm).subscribe(responce => {
            setTimeout(() => {
                this.alertService.stopLoadingMessage();
                this.isLoading = false;
                this.reset();
                this.alertService.showMessage('Register', `Successful registration!`, MessageSeverity.success);
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
    }

    reset() {
        this.formResetToggle = false;

        setTimeout(() => {
            this.formResetToggle = true;
        });
    }

}