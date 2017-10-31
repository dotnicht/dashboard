
import { Injectable, Injector } from '@angular/core';
import { Http, Headers, RequestOptions, Response, URLSearchParams } from '@angular/http';
import { Observable } from 'rxjs/Observable';
import { Subject } from 'rxjs/Subject';
import 'rxjs/add/observable/throw';
import 'rxjs/add/operator/map';
import 'rxjs/add/operator/mergeMap';
import 'rxjs/add/operator/catch';

import { AuthService } from './auth.service';
import { ConfigurationService } from './configuration.service';
import { UserLogin, UserRegister } from '../models/user.model';
import { CountryCode } from '../models/countryCodes';
import { MessageSeverity, AlertService } from './alert.service';


@Injectable()
export class EndpointFactory {

    static readonly apiVersion: string = '1';

    private readonly _loginUrl: string = '/connect/token';
    private readonly _logoutUrl: string = '/connect/logout';
    private readonly _registerUrl: string = '/connect/register';
    private readonly _isAuthUrl: string = '/connect/isauthorization';

    private get loginUrl() { return this.configurations.baseUrl + this._loginUrl; }

    private taskPauser: Subject<any>;
    private isRefreshingLogin: boolean;

    private _authService: AuthService;

    private get authService() {
        if (!this._authService)
            this._authService = this.injector.get(AuthService);

        return this._authService;
    }

    constructor(protected http: Http, protected configurations: ConfigurationService, private injector: Injector, protected alertService: AlertService) {

    }

    logoutEndPoint(): Observable<Response> {

        return this.http.post(this._logoutUrl, this.getAuthHeader())
            .map((response: Response) => {
                return response;
            })
            .catch(error => {
                return this.handleError(error, () => this.logoutEndPoint());
            });
    }
    registerEndPoint(user: UserRegister): Observable<Response> {
        let searchParams = new URLSearchParams();
        searchParams.append('Email', user.email);
        searchParams.append('Password', user.password);


        let headers = new Headers();
        headers.append('Content-Type', 'application/json');

        return this.http.post(this._registerUrl, JSON.stringify(user), { headers: headers })
            .map((response: Response) => {
                return response;
            })
            .catch((error: any) => {
                return Observable.throw(error);
            });

    }
    getLoginEndpoint(user: UserLogin): Observable<Response> {

        let header = new Headers();
        header.append('Content-Type', 'application/x-www-form-urlencoded');

        user.grant_type = 'password';
        user.scope = 'openid email phone profile offline_access roles';
        user.resource = window.location.origin;

        let searchParams = new URLSearchParams();
        searchParams.append('username', user.email);
        searchParams.append('password', user.password);
        searchParams.append('client_id', 'ID');
        searchParams.append('client_secret', '901564A5-E7FE-42CB-B10D-61EF6A8F3654');
        searchParams.append('grant_type', 'password');
        searchParams.append('scope', 'openid email phone profile offline_access roles');
        searchParams.append('resource', window.location.origin);

        let requestBody = searchParams.toString();

        return this.http.post(this.loginUrl, requestBody, { headers: header });
    }


    getRefreshLoginEndpoint(): Observable<Response> {

        let header = new Headers();
        header.append('Content-Type', 'application/x-www-form-urlencoded');


        let searchParams = new URLSearchParams();
        searchParams.append('refresh_token', this.authService.refreshToken);
        searchParams.append('client_id', 'ID');
        searchParams.append('client_secret', '901564A5-E7FE-42CB-B10D-61EF6A8F3654');
        searchParams.append('grant_type', 'refresh_token');
        searchParams.append('scope', 'openid profile offline_access');

        let requestBody = searchParams.toString();

        return this.http.post(this.loginUrl, requestBody, { headers: header })
            .map((response: Response) => {
                
                return response;
            })
            .catch(error => {
                return this.handleError(error, () => this.getRefreshLoginEndpoint());
            });
    }
    public isAuth(): Observable<Response> {
        return this.http.get(this._isAuthUrl, this.getAuthHeader())
            .map((response: Response) => {
                
                return response;
            })
            .catch(error => {
                return this.handleError(error, () => this.isAuth());
            });
    }

    getCountryCode() {
        return Observable.of(require('../assets/json/countryCodes.json'));
    }




    protected getAuthHeader(includeJsonContentType?: boolean): RequestOptions {
        let headers = new Headers({ 'Authorization': 'Bearer ' + this.authService.accessToken });

        if (includeJsonContentType)
            headers.append('Content-Type', 'application/json');

        headers.append('Accept', `application/vnd.iman.v${EndpointFactory.apiVersion}+json, application/json, text/plain, */*`);
        headers.append('App-Version', ConfigurationService.appVersion);

        return new RequestOptions({ headers: headers });
    }

    protected handleError(error, continuation: () => Observable<any>) {

        if (error.status == 401) {
            if (this.isRefreshingLogin) {
                return this.pauseTask(continuation);
            }

            this.isRefreshingLogin = true;

            return this.authService.refreshLogin()
                .mergeMap(data => {
                    this.isRefreshingLogin = false;
                    this.resumeTasks(true);

                    return continuation();
                })
                .catch(refreshLoginError => {
                    this.isRefreshingLogin = false;
                    this.resumeTasks(false);

                    if (refreshLoginError.status == 401 || (refreshLoginError.url && refreshLoginError.url.toLowerCase().includes(this.loginUrl.toLowerCase()))) {
                        this.authService.reLogin();
                        this.alertService.showMessage('Warning', `Session expired!`, MessageSeverity.warn);
                        return Observable.throw('session expired');
                    }
                    else {
                        return Observable.throw(refreshLoginError || 'server error');
                    }
                });
        }

        if (error.url && error.url.toLowerCase().includes(this.loginUrl.toLowerCase())) {
            this.authService.reLogin();
            return Observable.throw('session expired');
        }
        else {
            return Observable.throw(error || 'server error');
        }
    }



    private pauseTask(continuation: () => Observable<any>) {
        if (!this.taskPauser)
            this.taskPauser = new Subject();

        return this.taskPauser.switchMap(continueOp => {
            return continueOp ? continuation() : Observable.throw('session expired');
        });
    }


    private resumeTasks(continueOp: boolean) {
        setTimeout(() => {
            if (this.taskPauser) {
                this.taskPauser.next(continueOp);
                this.taskPauser.complete();
                this.taskPauser = null;
            }
        });
    }
}
