import { Injectable, Injector } from '@angular/core';
import { Http, Response } from '@angular/http';
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/map';

import { ConfigurationService } from './configuration.service';
import { Dashboard, PaymentType, IcoInfo } from '../models/dashboard.models';
import { Utilities } from './utilities';
import { AuthService } from './auth.service';
import { BaseService } from './base.service';
import { environment } from '../../environments/environment';
import { TokenTransfer } from '../models/tokenTransfer.model';


@Injectable()
export class DashboardEndpoint extends BaseService {

    private readonly _icoInfoUrl: string = environment.host + `/dashboard/ico_status`;
    private readonly _paymentTypesUrl: string = environment.host + `/dashboard/payment_status`;
    private readonly _dashboard: string = environment.host + `/dashboard/full_info`;
    private readonly _addQuestion: string = environment.host + `/dashboard/add_question`;
    private readonly _addTokenTransfer: string = environment.host + `/dashboard/add_token_transfer`;

    private subscription: any;

    constructor(authService: AuthService, http: Http, private configurations: ConfigurationService) {

        super(authService, http);
    }

    public getDashboard(): Observable<Response> {
        // let dashboard = new Dashboard();

        // return dashboard;
        let res = this.http.get(this._dashboard, this.authService.getAuthHeader())
            .map((response: Response) => {
                console.log('getDashboard()', response.json().headers);
                return response;
            })
            .catch(error => {

                return this.handleError(error, () => this.getDashboard());

            });
        return res;
    }

    public getIcoInfo(): Observable<Response> {

        let res = this.http.get(this._icoInfoUrl, this.authService.getAuthHeader())
            .map((response: Response) => {
                return response;
            })
            .catch(error => {

                return this.handleError(error, () => this.getIcoInfo());

            });
        return res;
    }
    public getPaymentTypes(): Observable<Response> {
        let res = this.http.get(this._paymentTypesUrl, this.authService.getAuthHeader())
            .map((response: Response) => {
                return response;
            })
            .catch(error => {

                return this.handleError(error, () => this.getPaymentTypes());

            });
        return res;
    }

    public addQuestion(text: string): Observable<Response> {

        let res = this.http.post(this._addQuestion, { Message: text }, this.authService.getAuthHeader())
            .map((response: Response) => {
                return response;
            })
            .catch(error => {

                return this.handleError(error, () => this.addQuestion(text));

            });
        return res;
    }
    public addtokenTransfer(model: TokenTransfer): Observable<Response> {

        let res = this.http.post(this._addTokenTransfer, model, this.authService.getAuthHeader())
            .map((response: Response) => {
                return response;
            })
            .catch(error => {

                return this.handleError(error, () => this.addtokenTransfer(model));

            });
        return res;
    }
}