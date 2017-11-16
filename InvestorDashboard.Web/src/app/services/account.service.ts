
import { Injectable } from '@angular/core';
import { Router, NavigationExtras } from '@angular/router';
import { Http, Headers, Response } from '@angular/http';
import { Observable } from 'rxjs/Observable';
import { Subject } from 'rxjs/Subject';
import 'rxjs/add/observable/forkJoin';
import 'rxjs/add/operator/do';
import 'rxjs/add/operator/map';

import { AccountEndpoint } from './account-endpoint.service';
import { AuthService } from './auth.service';
import { User } from '../models/user.model';
import { Role } from '../models/role.model';
import { Permission, PermissionNames, PermissionValues } from '../models/permission.model';
import { UserEdit } from '../models/user-edit.model';

@Injectable()
export class AccountService {


    constructor(private router: Router, private http: Http, private authService: AuthService,
        private accountEndpoint: AccountEndpoint) {

    }


    getUser(userId?: string) {

        return this.accountEndpoint.getUserEndpoint(userId)
            .map((response: Response) => <User>response.json());
    }

    updateUser(user: UserEdit) {
        return this.accountEndpoint.getUpdateUserEndpoint(user, user.id);
    }

    getUserPreferences() {

        return this.accountEndpoint.getUserPreferencesEndpoint()
            .map((response: Response) => <string>response.json());
    }

    updateUserPreferences(configuration: string) {
        return this.accountEndpoint.getUpdateUserPreferencesEndpoint(configuration);
    }

    refreshLoggedInUser() {
        return this.authService.refreshLogin();
    }

    get currentUser() {
        return this.authService.currentUser;
    }
}