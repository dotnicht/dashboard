import { Component, OnInit } from '@angular/core';
import { FormControl, Validators } from '@angular/forms';
import { UserRegister } from '../../models/User';
import { Http } from '@angular/http';
import { RegisterService } from '../../services/register.service';

@Component({
    selector: 'app-register',
    templateUrl: './register.component.html',
    styleUrls: ['./register.component.scss']
})

export class RegisterComponent implements OnInit {
    formResetToggle = true;
    registerForm = new UserRegister();

    constructor(private registerService: RegisterService) {

    }


    /** Called by Angular after register component initialized */
    ngOnInit(): void {
        this.registerForm.email = 'denis.skvortsow@gmail.com';
        this.registerForm.password = '123456_Kol';
        this.registerForm.confirmPassword = '123456_Kol';
    }

    OnSubmit() {
        this.registerService.send(this.registerForm).subscribe(responce => {
         console.log(responce);
        });
    }

    reset() {
        this.formResetToggle = false;

        setTimeout(() => {
            this.formResetToggle = true;
        });
    }

}