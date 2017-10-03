import { Component, OnInit, OnDestroy, Input } from '@angular/core';
import { UserLogin } from '../../models/User';
import { AuthService } from '../../services/auth.service';

@Component({
    selector: 'app-login',
    templateUrl: './login.component.html',
    styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit, OnDestroy {


    userLogin = new UserLogin();
    formResetToggle = true;

    constructor(private authService: AuthService) { }

    ngOnInit(): void {
        this.userLogin.email = 'denis.skvortsow@gmail.com';
        this.userLogin.password = '123456_Kol';
    }
    ngOnDestroy(): void {

    }
    login() {
        this.authService.send(this.userLogin).subscribe(responce => {
            console.log(responce);
        });
    }
}