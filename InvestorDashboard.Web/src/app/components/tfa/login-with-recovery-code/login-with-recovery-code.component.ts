import { Component } from '@angular/core';

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
    constructor() {

    }
}