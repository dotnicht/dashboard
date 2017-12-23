import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../services/auth.service';
import { Utilities } from '../../../services/utilities';

class RecoveryCode {
    value: string;
}

@Component({
    selector: 'app-login-with-recovery-code',
    templateUrl: './login-with-recovery-code.component.html',
    styleUrls: ['./login-with-recovery-code.component.scss']
})
/** LoginWithRecoveryCode component*/
export class LoginWithRecoveryCodeComponent {
    isLoading: false;
    errors: string;
    recoveryCode: RecoveryCode = new RecoveryCode;
    constructor(private router: Router,
        private authService: AuthService) {

    }
    OnSubmit() {
        this.authService.loginWithRecoveryCode(this.recoveryCode.value).subscribe(data => {
            this.router.navigate(['/']);
        },
            errors => {
                this.errors = Utilities.findHttpResponseMessage('error_description', errors);
            });
    }
}