import { Component } from '@angular/core';
import { ChangePassWord } from '../../../models/user-edit.model';

@Component({
    selector: 'app-reset-password',
    templateUrl: './reset-password.component.html',
    styleUrls: ['./reset-password.component.scss']
})
/** reset-password component*/
export class ResetPasswordComponent {
    public changePassWordForm = new ChangePassWord();
    isLoading = false;
    errorMsg: string;
    constructor() {

    }
    
    OnSubmit() {

    }
}