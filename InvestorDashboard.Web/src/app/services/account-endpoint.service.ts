import { Injectable, Injector } from '@angular/core';
import { Http, Response } from '@angular/http';
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/map';

import { ConfigurationService } from './configuration.service';
import { BaseService } from './base.service';
import { AuthService } from './auth.service';
import { environment } from '../../environments/environment';
import { ForgotPassWord, ChangePassWord, ResetPassword } from '../models/user-edit.model';
import { ResendEmailConfirmCode } from '../components/controls/resend-email-confirm-code/resend-email-confirm-code.component';
import { ContentType } from '../models/content-type-enum.model';
import { HttpClient } from '@angular/common/http';


@Injectable()
export class AccountEndpoint {

    private readonly _usersUrl: string = environment.host + '/account/users';
    private readonly _currentUserUrl: string = environment.host + '/account/users/me';
    private readonly _currentUserPreferencesUrl: string = environment.host + '/account/users/me/preferences';
    private readonly _forgotPasswordUrl: string = environment.host + '/account/forgot_password';
    private readonly _changePasswordUrl: string = environment.host + '/account/change_password';
    private readonly _resetPasswordUrl: string = environment.host + '/account/reset_password';
    private readonly _tfaEnableUrl: string = environment.host + '/account/tfa_enable';
    private readonly _tfaUrl: string = environment.host + '/account/tfa';
    private readonly _tfaGetRecoveryCodes: string = environment.host + '/account/get_tf_recovery_codes';
    private readonly _tfaDisable: string = environment.host + '/account/tfa_disable';
    private readonly _tfaReset: string = environment.host + '/account/tfa_reset';
    private readonly _resendEmailConfirmCode: string = environment.host + '/connect/resend_email_confirm_code';

    private readonly _ethAddress: string = environment.host + '/dashboard/token';


    get usersUrl() { return this._usersUrl; }
    get currentUserUrl() { return this._currentUserUrl; }
    get currentUserPreferencesUrl() { return this._currentUserPreferencesUrl; }


    constructor(private http: HttpClient, private authService: AuthService, private configurations: ConfigurationService) {
    }


    forgotPasswordEndpoint(form: ForgotPassWord) {
        const res = this.http.post(this._forgotPasswordUrl, form);
        return res;
    }
    resendEmailConfirmCodeEndpoint(form: ResendEmailConfirmCode) {
        const res = this.http.post(this._resendEmailConfirmCode, form);
        return res;
    }
    changePasswordEndpoint(form: ChangePassWord) {
        console.log(form);
        const res = this.http.post(this._changePasswordUrl, form, this.authService.getAuthHeader());
        return res;
    }
    resetPasswordEndpoint(form: ResetPassword) {
        console.log(form);
        const res = this.http.post(this._resetPasswordUrl, form, this.authService.getAuthHeader());
        return res;
    }

    getUserEndpoint(userId?: string) {
        const endpointUrl = userId ? `${this.usersUrl}/${userId}` : this.currentUserUrl;

        const res = this.http.get(endpointUrl, this.authService.getAuthHeader());
        return res;
    }

    getUpdateUserEndpoint(userObject: any, userId?: string) {
        const endpointUrl = this.currentUserUrl;
        return this.http.put(endpointUrl, JSON.stringify(userObject), this.authService.getAuthHeader());
    }

    getPatchUpdateUserEndpoint(patch: {}, userId?: string);
    getPatchUpdateUserEndpoint(value: any, op: string, path: string, from?: any, userId?: string);
    getPatchUpdateUserEndpoint(valueOrPatch: any, opOrUserId?: string, path?: string, from?: any, userId?: string) {
        let endpointUrl: string;
        let patchDocument: {};

        if (path) {
            endpointUrl = userId ? `${this.usersUrl}/${userId}` : this.currentUserUrl;
            patchDocument = from ?
                [{ 'value': valueOrPatch, 'path': path, 'op': opOrUserId, 'from': from }] :
                [{ 'value': valueOrPatch, 'path': path, 'op': opOrUserId }];
        } else {
            endpointUrl = opOrUserId ? `${this.usersUrl}/${opOrUserId}` : this.currentUserUrl;
            patchDocument = valueOrPatch;
        }

        return this.http.patch(endpointUrl, JSON.stringify(patchDocument), this.authService.getAuthHeader());
    }


    getUserPreferencesEndpoint() {

        return this.http.get(this.currentUserPreferencesUrl, this.authService.getAuthHeader());
    }

    getUpdateUserPreferencesEndpoint(configuration: string) {
        return this.http.put(this.currentUserPreferencesUrl, JSON.stringify(configuration), this.authService.getAuthHeader());
    }


    TfGetActivationDataEndpoint() {
        const res = this.http.get(this._tfaEnableUrl, this.authService.getAuthHeader());
        return res;
    }
    TfPostActivationDataEndpoint(code: string) {
        const res = this.http.post(this._tfaEnableUrl, JSON.stringify({ code: code }), this.authService.getAuthHeader());
        return res;
    }
    TfaDataEndpoint() {
        const res = this.http.get(this._tfaUrl, this.authService.getAuthHeader());
        return res;
    }
    TfGetRecoveryCodesEndpoint() {
        const res = this.http.get(this._tfaGetRecoveryCodes, this.authService.getAuthHeader());
        return res;
    }
    TfDisableEndpoint() {
        const res = this.http.post(this._tfaDisable, null, this.authService.getAuthHeader());
        return res;
    }
    TfResetEndpoint() {
        const res = this.http.post(this._tfaReset, null, this.authService.getAuthHeader());
        return res;
    }

    updateEthAddress(address: string) {

        const res = this.http.post(this._ethAddress,
            JSON.stringify({ address: address }), this.authService.getAuthHeader());
        return res;
    }
    getEthAddress() {
        const res = this.http.get(this._ethAddress, this.authService.getAuthHeader());
        return res;
    }
}
