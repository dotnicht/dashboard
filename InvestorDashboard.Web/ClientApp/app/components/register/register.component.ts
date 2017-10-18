import { Component, OnInit } from '@angular/core';
import { FormControl, Validators } from '@angular/forms';
import { UserRegister } from '../../models/user.model';
import { Http } from '@angular/http';
import { AuthService } from '../../services/auth.service';
import { AlertService, MessageSeverity } from '../../services/alert.service';

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
        this.authService.register(this.registerForm).subscribe(responce => {
            setTimeout(() => {
                this.alertService.stopLoadingMessage();
                this.isLoading = false;
                this.reset();
                this.alertService.showMessage('Register', `Successful registration!`, MessageSeverity.success);
            }, 500);
        });
    }

    reset() {
        this.formResetToggle = false;

        setTimeout(() => {
            this.formResetToggle = true;
        });
    }

}