

import { Injectable } from '@angular/core';
import { Router, NavigationExtras } from '@angular/router';
import { Http, Headers, RequestOptions, Response, URLSearchParams } from '@angular/http';
import { Observable } from 'rxjs/Observable';
import { Subject } from 'rxjs/Subject';
import 'rxjs/add/operator/map';

import { LocalStoreManager } from './local-store-manager.service';
import { ConfigurationService } from './configuration.service';
import { DBkeys } from './db-Keys';
import { JwtHelper } from './jwt-helper';
import { Utilities } from './utilities';
import { UserLogin, User, UserRegister } from '../models/user.model';
import { Permission, PermissionNames, PermissionValues } from '../models/permission.model';
import { environment } from '../../environments/environment';

@Injectable()
export class AuthService {

  private readonly _logoutUrl: string = '/connect/logout';
  private readonly _registerUrl: string = '/connect/register';
  private readonly _loginTfa: string = environment.host + '/connect/login2fa';
  private readonly _loginWithRecoveryCode: string = environment.host + '/connect/login_with_recovery_code';

  private isRefreshingLogin: boolean;

  public get loginUrl() { return '/connect/token'; }
  public get homeUrl() { return this.configurations.homeUrl; }

  public loginRedirectUrl: string;
  public logoutRedirectUrl: string = '/login';

  public reLoginDelegate: () => void;

  private previousIsLoggedInCheck = false;
  private _loginStatus = new Subject<boolean>();
  private isAuth: boolean;

  gotoPage(page: string, preserveParams = true) {

    const navigationExtras: NavigationExtras = {
      queryParamsHandling: preserveParams ? 'merge' : '', preserveFragment: preserveParams
    };


    this.router.navigate([page], navigationExtras);
  }

  getAuthHeader(includeJsonContentType?: boolean): RequestOptions {
    const headers = new Headers({ 'Authorization': 'Bearer ' + this.accessToken });

    if (includeJsonContentType) {
      headers.append('Content-Type', 'application/json');
    }

    headers.append('Accept', `application/vnd.iman.v1+json, application/json, text/plain, */*`);
    headers.append('App-Version', ConfigurationService.appVersion);

    return new RequestOptions({ headers: headers });
  }

  logoutEndPoint(): Observable<Response> {

    return this.http.post(environment.host + this._logoutUrl, this.getAuthHeader())
      .map((response: Response) => {
        return response;
      })
      .catch(error => {
        return this.handleError(error, () => this.logoutEndPoint());
      });
  }

  getRefreshLoginEndpoint(): Observable<Response> {

    const header = new Headers();
    header.append('Content-Type', 'application/x-www-form-urlencoded');


    const searchParams = new URLSearchParams();
    searchParams.append('refresh_token', this.refreshToken);
    searchParams.append('client_id', 'ID');
    searchParams.append('client_secret', '901564A5-E7FE-42CB-B10D-61EF6A8F3654');
    searchParams.append('grant_type', 'refresh_token');
    searchParams.append('scope', 'openid profile offline_access');

    const requestBody = searchParams.toString();

    return this.http.post(environment.host + this.loginUrl, requestBody, { headers: header })
      .map((response: Response) => {

        return response;
      })
      .catch(error => {
        return this.handleError(error, () => this.getRefreshLoginEndpoint());
      });
  }
  redirectLoginUser() {
    let redirect = this.loginRedirectUrl &&
      this.loginRedirectUrl != '/' &&
      this.loginRedirectUrl != ConfigurationService.defaultHomeUrl ? this.loginRedirectUrl : this.homeUrl;

    let urlParamsAndFragment = Utilities.splitInTwo(redirect, '#');
    let urlAndParams = Utilities.splitInTwo(urlParamsAndFragment.firstPart, '?');

    let navigationExtras: NavigationExtras = {
      fragment: urlParamsAndFragment.secondPart,
      queryParams: Utilities.getQueryParamsFromString(urlAndParams.secondPart),
      queryParamsHandling: 'merge'
    };

    this.router.navigate([urlAndParams.firstPart], navigationExtras);
  }


  redirectLogoutUser() {
    let redirect = this.logoutRedirectUrl;

    this.router.navigate([redirect]);
  }


  redirectForLogin() {
    this.loginRedirectUrl = this.router.url;
    this.router.navigate([this.loginUrl]);
  }


  reLogin() {

    this.localStorage.deleteData(DBkeys.TOKEN_EXPIRES_IN);

    if (this.reLoginDelegate) {
      this.reLoginDelegate();
    } else {
      this.redirectForLogin();
    }
  }
  private handleError(error, continuation: () => Observable<any>) {

    if (error.status == 401) {
      // this.logout();
      // this.redirectLogoutUser();
      this.refreshLogin();
    }

    if (error.url && error.url.toLowerCase().includes(this.loginUrl.toLowerCase())) {
      // this.logout();
      // this.redirectLogoutUser();
      this.refreshLogin();
      return Observable.throw('session expired');
    }
    else {
      return Observable.throw(error || 'server error');
    }
  }

  refreshLogin() {
    return this.getRefreshLoginEndpoint()
      .map((response: Response) => this.processLoginResponse(response, this.rememberMe))
      .catch((error: any) => {
        return Observable.throw(error);
      });
  }


  login(user: UserLogin) {

    if (this.isLoggedIn) {
      this.logout();
    }

    return this.getLoginEndpoint(user)
      .map((response: Response) => this.processLoginResponse(response, user.rememberMe))
      .catch((error: any) => {
        return Observable.throw(error);
      });
  }
  loginWithTfa(code: string) {
    const headers = new Headers();
    headers.append('Content-Type', 'application/json');

    return this.http.post(this._loginTfa, JSON.stringify({ TwoFactorCode: code }), this.getAuthHeader(true))
      .map((response: Response) => {
        let user = this.currentUser;
        user.twoFactorValidated = true;
        this.localStorage.saveSyncedSessionData(user, DBkeys.CURRENT_USER);
        return response;
      })
      .catch((error: any) => {
        return Observable.throw(error);
      });
  }
  loginWithRecoveryCode(code: string) {
    const headers = new Headers();
    headers.append('Content-Type', 'application/json');

    return this.http.post(this._loginWithRecoveryCode, JSON.stringify({ RecoveryCode: code }), this.getAuthHeader(true))
      .map((response: Response) => {
        let user = this.currentUser;
        user.twoFactorValidated = true;
        this.localStorage.saveSyncedSessionData(user, DBkeys.CURRENT_USER);
        return response;
      })
      .catch((error: any) => {
        return Observable.throw(error);
      });
  }

  logout(): void {
    this.logoutEndPoint();
    this.clearUser();

  }
  register(user: UserRegister): Observable<Response> {
    let searchParams = new URLSearchParams();
    searchParams.append('Email', user.email);
    searchParams.append('Password', user.password);


    let headers = new Headers();
    headers.append('Content-Type', 'application/json');

    return this.http.post(environment.host + this._registerUrl, JSON.stringify(user), { headers: headers })
      .map((response: Response) => {
        return response;
      })
      .catch((error: any) => {
        return Observable.throw(error);
      });

  }
  getLoginStatusEvent(): Observable<boolean> {
    return this._loginStatus.asObservable();
  }
  getLoginEndpoint(user: UserLogin): Observable<Response> {

    let header = new Headers();
    header.append('Content-Type', 'application/x-www-form-urlencoded');

    user.grant_type = 'password';
    user.scope = 'openid email phone profile offline_access roles';
    user.resource = window.location.origin;
    let searchParams = new URLSearchParams();

    searchParams.append('username', encodeURIComponent(user.email));
    searchParams.append('password', user.password);
    searchParams.append('client_id', 'ID');
    searchParams.append('client_secret', '901564A5-E7FE-42CB-B10D-61EF6A8F3654');
    searchParams.append('grant_type', 'password');
    searchParams.append('scope', 'openid email phone profile offline_access roles');
    searchParams.append('resource', window.location.origin);

    let requestBody = searchParams.toString();

    return this.http.post(environment.host + this.loginUrl, requestBody, { headers: header });
  }
  constructor(private router: Router,
    private http: Http,
    private configurations: ConfigurationService,
    private localStorage: LocalStoreManager) {
    this.initializeLoginStatus();
  }


  private initializeLoginStatus() {
    this.localStorage.getInitEvent().subscribe(() => {
      this.reevaluateLoginStatus();
    });
  }


  private clearUser() {
    this.localStorage.deleteData(DBkeys.ACCESS_TOKEN);
    this.localStorage.deleteData(DBkeys.ID_TOKEN);
    this.localStorage.deleteData(DBkeys.REFRESH_TOKEN);
    this.localStorage.deleteData(DBkeys.TOKEN_EXPIRES_IN);
    this.localStorage.deleteData(DBkeys.USER_PERMISSIONS);
    this.localStorage.deleteData(DBkeys.CURRENT_USER);

    this.configurations.clearLocalChanges();

    this.reevaluateLoginStatus();
  }


  private processLoginResponse(response: Response, rememberMe: boolean, tfValidate: boolean = false) {

    const response_token = response.json();
    const accessToken = response_token.access_token;

    if (accessToken == null) {
      throw new Error('Received accessToken was empty');
    }

    const idToken: string = response_token.id_token;
    const refreshToken: string = response_token.refresh_token;
    const expiresIn: number = response_token.expires_in;

    const tokenExpiryDate = new Date();
    tokenExpiryDate.setSeconds(tokenExpiryDate.getSeconds() + expiresIn);

    const accessTokenExpiry = tokenExpiryDate;

    const jwtHelper = new JwtHelper();
    const decodedIdToken = jwtHelper.decodeToken(response_token.id_token);

    const permissions: PermissionValues[] =
      Array.isArray(decodedIdToken.permission) ? decodedIdToken.permission : [decodedIdToken.permission];

    if (!this.isLoggedIn) {
      this.configurations.import(decodedIdToken.configuration);
    }

    const user = new User(

      decodedIdToken.sub,
      decodedIdToken.name,
      decodedIdToken.email,
      decodedIdToken.twofactorenabled);

    user.isEnabled = true;

    console.log(decodedIdToken);
    this.saveUserDetails(user, permissions, accessToken, idToken, refreshToken, accessTokenExpiry, rememberMe);

    this.reevaluateLoginStatus(user);

    return user;
  }


  private saveUserDetails(user: User,
    permissions: PermissionValues[],
    accessToken: string, idToken: string,
    refreshToken: string, expiresIn: Date,
    rememberMe: boolean) {

    if (rememberMe) {
      this.localStorage.savePermanentData(accessToken, DBkeys.ACCESS_TOKEN);
      this.localStorage.savePermanentData(idToken, DBkeys.ID_TOKEN);
      this.localStorage.savePermanentData(refreshToken, DBkeys.REFRESH_TOKEN);
      this.localStorage.savePermanentData(expiresIn, DBkeys.TOKEN_EXPIRES_IN);
      this.localStorage.savePermanentData(permissions, DBkeys.USER_PERMISSIONS);
      this.localStorage.savePermanentData(user, DBkeys.CURRENT_USER);
    } else {
      this.localStorage.saveSyncedSessionData(accessToken, DBkeys.ACCESS_TOKEN);
      this.localStorage.saveSyncedSessionData(idToken, DBkeys.ID_TOKEN);
      this.localStorage.saveSyncedSessionData(refreshToken, DBkeys.REFRESH_TOKEN);
      this.localStorage.saveSyncedSessionData(expiresIn, DBkeys.TOKEN_EXPIRES_IN);
      this.localStorage.saveSyncedSessionData(permissions, DBkeys.USER_PERMISSIONS);
      this.localStorage.saveSyncedSessionData(user, DBkeys.CURRENT_USER);
    }

    this.localStorage.savePermanentData(rememberMe, DBkeys.REMEMBER_ME);
  }



  private reevaluateLoginStatus(currentUser?: User) {

    const user = currentUser || this.localStorage.getDataObject<User>(DBkeys.CURRENT_USER);
    const isLoggedIn = user != null;

    if (this.previousIsLoggedInCheck != isLoggedIn) {
      setTimeout(() => {
        this._loginStatus.next(isLoggedIn);
      });
    }

    this.previousIsLoggedInCheck = isLoggedIn;
  }

  get currentUser(): User {

    const user = this.localStorage.getDataObject<User>(DBkeys.CURRENT_USER);
    this.reevaluateLoginStatus(user);

    return user;
  }

  get userPermissions(): PermissionValues[] {
    return this.localStorage.getDataObject<PermissionValues[]>(DBkeys.USER_PERMISSIONS) || [];
  }

  get accessToken(): string {

    this.reevaluateLoginStatus();
    return this.localStorage.getData(DBkeys.ACCESS_TOKEN);
  }

  get accessTokenExpiryDate(): Date {

    this.reevaluateLoginStatus();
    return this.localStorage.getDataObject<Date>(DBkeys.TOKEN_EXPIRES_IN, true);
  }

  get isSessionExpired(): boolean {

    if (this.accessTokenExpiryDate == null) {
      return true;
    }

    return !(this.accessTokenExpiryDate.valueOf() > new Date().valueOf());
  }


  get idToken(): string {

    this.reevaluateLoginStatus();
    return this.localStorage.getData(DBkeys.ID_TOKEN);
  }

  get refreshToken(): string {

    this.reevaluateLoginStatus();
    return this.localStorage.getData(DBkeys.REFRESH_TOKEN);
  }

  get isLoggedIn(): boolean {
    // return this.currentUser != null;
    // setTimeout(() => {
    //   this.endpointFactory.isAuth().subscribe(resp => {
    //     // console.log(resp.json());
    //     this.isAuth = resp.json();

    //   });
    // });
    if (this.currentUser != null) {
      if (this.currentUser.twoFactorEnabled) {
        if (this.currentUser.twoFactorValidated) {
          return true;
        }
        return false;
      }
      return true;
    }
    return false;
    // return this.isAuth && (this.currentUser != null);
  }

  get rememberMe(): boolean {
    return this.localStorage.getDataObject<boolean>(DBkeys.REMEMBER_ME) == true;
  }
}
