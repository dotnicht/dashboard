import { Injectable, Injector } from '@angular/core';
import { Http, Response } from '@angular/http';
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/map';

import { ConfigurationService } from './configuration.service';
import { BaseService } from './base.service';
import { AuthService } from './auth.service';
import { environment } from '../../environments/environment';
import { ForgotPassWord, ChangePassWord, ResetPassword } from '../models/user-edit.model';


@Injectable()
export class AccountEndpoint extends BaseService {

    private readonly _usersUrl: string = environment.host + '/account/users';
    private readonly _currentUserUrl: string = environment.host + '/account/users/me';
    private readonly _currentUserPreferencesUrl: string = environment.host + '/account/users/me/preferences';
    private readonly _forgotPasswordUrl: string = environment.host + '/account/forgot_password';
    private readonly _changePasswordUrl: string = environment.host + '/account/change_password';
    private readonly _resetPasswordUrl: string = environment.host + '/account/reset_password';


    get usersUrl() { return this._usersUrl; }
    get currentUserUrl() { return this._currentUserUrl; }
    get currentUserPreferencesUrl() { return this._currentUserPreferencesUrl; }


    constructor(http: Http, authService: AuthService, private configurations: ConfigurationService) {

        super(authService, http);
    }


    forgotPasswordEndpoint(form: ForgotPassWord): Observable<Response> {
        const res = this.http.post(this._forgotPasswordUrl, form)
            .map((response: Response) => {
                return response;
            })
            .catch(error => {
                return this.handleError(error, () => this.forgotPasswordEndpoint(form));

            });
        return res;
    }
    changePasswordEndpoint(form: ChangePassWord): Observable<Response> {
        console.log(form);
        const res = this.http.post(this._changePasswordUrl, form, this.authService.getAuthHeader())
            .map((response: Response) => {
                return response;
            })
            .catch(error => {
                return this.handleError(error, () => this.changePasswordEndpoint(form));

            });
        return res;
    }
    resetPasswordEndpoint(form: ResetPassword): Observable<Response> {
        console.log(form);
        const res = this.http.post(this._resetPasswordUrl, form, this.authService.getAuthHeader())
            .map((response: Response) => {
                return response;
            })
            .catch(error => {
                return this.handleError(error, () => this.resetPasswordEndpoint(form));

            });
        return res;
    }

    getUserEndpoint(userId?: string): Observable<Response> {
        const endpointUrl = userId ? `${this.usersUrl}/${userId}` : this.currentUserUrl;

        const res = this.http.get(endpointUrl, this.authService.getAuthHeader())
            .map((response: Response) => {
                return response;
            })
            .catch(error => {

                return this.handleError(error, () => this.getUserEndpoint(userId));

            });
        return res;
    }

    getUpdateUserEndpoint(userObject: any, userId?: string): Observable<Response> {
        const endpointUrl = this.currentUserUrl;
        return this.http.put(endpointUrl, JSON.stringify(userObject), this.authService.getAuthHeader(true))
            .map((response: Response) => {
                return response;
            })
            .catch(error => {
                return this.handleError(error, () => this.getUpdateUserEndpoint(userObject, userId));
            });
    }

    getPatchUpdateUserEndpoint(patch: {}, userId?: string): Observable<Response>;
    getPatchUpdateUserEndpoint(value: any, op: string, path: string, from?: any, userId?: string): Observable<Response>;
    getPatchUpdateUserEndpoint(valueOrPatch: any, opOrUserId?: string, path?: string, from?: any, userId?: string): Observable<Response> {
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

        return this.http.patch(endpointUrl, JSON.stringify(patchDocument), this.authService.getAuthHeader(true))
            .map((response: Response) => {
                return response;
            })
            .catch(error => {
                return this.handleError(error, () => this.getPatchUpdateUserEndpoint(valueOrPatch, opOrUserId, path, from, userId));
            });
    }


    getUserPreferencesEndpoint(): Observable<Response> {

        return this.http.get(this.currentUserPreferencesUrl, this.authService.getAuthHeader())
            .map((response: Response) => {
                return response;
            })
            .catch(error => {
                return this.handleError(error, () => this.getUserPreferencesEndpoint());
            });
    }

    getUpdateUserPreferencesEndpoint(configuration: string): Observable<Response> {
        return this.http.put(this.currentUserPreferencesUrl, JSON.stringify(configuration), this.authService.getAuthHeader(true))
            .map((response: Response) => {
                return response;
            })
            .catch(error => {
                return this.handleError(error, () => this.getUpdateUserPreferencesEndpoint(configuration));
            });
    }

}
