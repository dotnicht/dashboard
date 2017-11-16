import { AuthService } from './auth.service';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs/Observable';
import { Subject } from 'rxjs/Subject';
import { Http } from '@angular/http';

import 'rxjs/add/observable/throw';
import 'rxjs/add/operator/map';
import 'rxjs/add/operator/mergeMap';
import 'rxjs/add/operator/catch';


@Injectable()
export class BaseService {

    private taskPauser: Subject<any>;
    private isRefreshingLogin: boolean;

    constructor(protected authService: AuthService, protected http: Http) { }

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

                    if (refreshLoginError.status == 401 ||
                        (refreshLoginError.url && refreshLoginError.url.toLowerCase().includes(this.authService.loginUrl.toLowerCase()))) {
                        this.authService.reLogin();
                        //this.alertService.showMessage('Warning', `Session expired!`, MessageSeverity.warn);
                        return Observable.throw('session expired');
                    }
                    else {
                        return Observable.throw(refreshLoginError || 'server error');
                    }
                });
        }

        if (error.url && error.url.toLowerCase().includes(this.authService.loginUrl.toLowerCase())) {
            this.authService.reLogin();
            return Observable.throw('session expired');
        }
        else {
            return Observable.throw(error || 'server error');
        }
    }
    private pauseTask(continuation: () => Observable<any>) {
        if (!this.taskPauser) {
            this.taskPauser = new Subject();
        }

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