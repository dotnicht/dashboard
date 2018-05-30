import { Injectable } from '@angular/core';
import { AuthStateModel } from '../models/auth-state-model';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { Subscription } from 'rxjs/Subscription';
import { Observable } from 'rxjs/Observable';
import { AuthTokenModel } from '../models/auth-tokens-model';
// import { ProfileModel } from '../models/profile-model';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { UserRegister, UserLogin, User } from '../models/user.model';

import { JwtHelper } from './jwt-helper';
import 'rxjs/add/operator/map';
import 'rxjs/add/operator/mergeMap';
import 'rxjs/add/operator/first';
import 'rxjs/add/operator/catch';
import 'rxjs/add/operator/do';
import 'rxjs/add/operator/filter';

import 'rxjs/add/observable/of';
import 'rxjs/add/observable/interval';
import 'rxjs/add/observable/throw';
import { PermissionValues } from '../models/permission.model';
import { LocalStoreManager } from './local-store-manager.service';
import { DBkeys } from './db-Keys';
import { Router } from '@angular/router';
import { RefreshGrantModel } from '../models/refresh-grant-model';


@Injectable()
export class AuthService {

    private initalState: AuthStateModel = { profile: null, tokens: null, authReady: false };
    private authReady$ = new BehaviorSubject<boolean>(false);
    private state = new BehaviorSubject<AuthStateModel>(this.initalState);
    private refreshSubscription$: Subscription;

    state$ = this.state.asObservable();
    tokens$: Observable<AuthTokenModel>;
    profile$: Observable<User>;
    loggedIn$: Observable<boolean>;


    loggedIn = false;

    constructor(
        private http: HttpClient,
        private localStorage: LocalStoreManager,
        private router: Router
    ) {



        this.tokens$ = this.state.filter(state => state.authReady)
            .map(state => state.tokens);

        this.profile$ = this.state.filter(state => state.authReady)
            .map(state => state.profile);

        this.loggedIn$ = this.tokens$.map(tokens => !!tokens);

        this.state$.subscribe(data => {
            if (data.authReady) {
                this.loggedIn = data.tokens != null;
                if (data.tokens == null) {
                    this.router.navigate(['/login']);
                }
            }

        });
    }
    private accessToken() {
        const tokensString = this.localStorage.getData(DBkeys.AUTH_TOKENS);

        const tokensModel: AuthTokenModel = tokensString == null ? null : JSON.parse(tokensString);
        if (tokensModel) {

            return tokensModel.access_token;
        } else {
            return null;
        }
    }
    get currentUser(): User {

        const user = this.localStorage.getDataObject<User>(DBkeys.CURRENT_USER);

        return user;
    }
    isLoggedIn() {
        // .map(state => state.tokens) != null)
        return this.loggedIn;
    }


    getAuthHeader(includeJsonContentType?: boolean) {
        const headers = new HttpHeaders({
            'Content-Type': 'application/json',
            'Authorization': 'Bearer ' + this.accessToken(),
            'Accept': `application/vnd.iman.v1+json, application/json, text/plain, */*`,
            'App-Version': '1.0.0'
        });
        const httpOptions = {
            headers: headers
        };
        return httpOptions;
    }
    init(): Observable<AuthTokenModel> {
        return this.startupTokenRefresh()
            .do(() => this.scheduleRefresh());
    }

    register(data: UserRegister) {
        return this.http.post(`${environment.host}/connect/register`, data);
    }

    login(user: UserLogin): Observable<any> {
        return this.getTokens(user, 'password')
            .catch(res => Observable.throw(res))
            .do(res => this.scheduleRefresh());
    }

    logout(): void {
        this.updateState({ profile: null, tokens: null });
        if (this.refreshSubscription$) {
            this.refreshSubscription$.unsubscribe();
        }
        this.removeToken();
    }

    refreshTokens(): Observable<AuthTokenModel> {
        const refreshModel = new RefreshGrantModel();
        return this.state.first()
            .map(state => state.tokens)
            .flatMap(tokens => this.getTokens({
                refresh_token: tokens.refresh_token,
                client_id: 'ID',
                client_secret: '901564A5-E7FE-42CB-B10D-61EF6A8F3654',
                scope: 'openid profile offline_access',
                resource: window.location.origin
            }, 'refresh_token')
                .catch(error => Observable.throw('Session Expired'))
            );
    }

    private storeToken(tokens: AuthTokenModel): void {
        const previousTokens = this.retrieveTokens();
        if (previousTokens != null && tokens.refresh_token == null) {
            tokens.refresh_token = previousTokens.refresh_token;
        }

        // localStorage.setItem('auth-tokens', JSON.stringify(tokens));
        this.localStorage.savePermanentData(JSON.stringify(tokens), DBkeys.AUTH_TOKENS);


    }

    private retrieveTokens(): AuthTokenModel {
        const tokensString = this.localStorage.getData(DBkeys.AUTH_TOKENS);
        const tokensModel: AuthTokenModel = tokensString == null ? null : JSON.parse(tokensString);
        return tokensModel;
    }

    private removeToken(): void {
        this.localStorage.deleteData(DBkeys.AUTH_TOKENS);
    }

    private updateState(newState: AuthStateModel): void {
        const previousState = this.state.getValue();
        this.state.next(Object.assign({}, previousState, newState));
    }

    private getTokens(data: RefreshGrantModel | UserLogin, grantType: string): Observable<any> {

        const httpOptions = {
            headers: new HttpHeaders({
                'Content-Type': 'application/x-www-form-urlencoded'
            })
        };

        Object.assign(data, { grant_type: grantType, scope: 'openid email phone profile offline_access roles' });

        const params = new URLSearchParams();
        Object.keys(data)
            .forEach(key => params.append(key, data[key]));


        return this.http.post<AuthTokenModel>(`${environment.host}/connect/token`, params.toString(), httpOptions)
            .map(res => {
                const tokens: AuthTokenModel = res;
                const now = new Date();

                tokens.expiration_date = new Date(now.getTime() + tokens.expires_in * 1000).getTime().toString();

                const jwtHelper = new JwtHelper();

                const configuration = new User();

                const profile = jwtHelper.decodeToken(tokens.id_token);

                Object.assign(configuration, profile);

                const permissions: PermissionValues[] = Array.isArray(profile.role) ? profile.role : [profile.role];

                this.localStorage.savePermanentData(profile, DBkeys.CURRENT_USER);
                this.localStorage.savePermanentData(permissions, DBkeys.USER_PERMISSIONS);

                this.storeToken(tokens);

                this.updateState({ authReady: true, tokens, profile });

                this.router.navigateByUrl('/');
            });
    }

    private startupTokenRefresh(): Observable<AuthTokenModel> {
        return Observable.of(this.retrieveTokens())
            .flatMap((tokens: AuthTokenModel) => {
                if (!tokens) {
                    this.updateState({ authReady: false });
                    return [];
                    // return Observable.throw('No token in Storage');
                } else {
                    const jwtHelper = new JwtHelper();
                    const profile: User = jwtHelper.decodeToken(tokens.id_token);
                    this.updateState({ tokens, profile });

                    if (+tokens.expiration_date > new Date().getTime()) {
                        this.updateState({ authReady: false });
                    }
                    return this.refreshTokens();
                }



            })
            .catch(error => {
                console.log(error);
                this.logout();
                this.updateState({ authReady: false });
                return Observable.throw(error);
            });
    }

    private scheduleRefresh(): void {
        this.refreshSubscription$ = this.tokens$
            .first()
            // refresh every half the total expiration time
            .flatMap(tokens => Observable.interval(tokens.expires_in / 2 * 1000))
            .flatMap(() => this.refreshTokens())
            .subscribe();
    }
}
