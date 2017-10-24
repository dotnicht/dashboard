import { Injectable, Injector } from '@angular/core';
import { Http, Response } from '@angular/http';
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/map';

import { EndpointFactory } from './endpoint-factory.service';
import { ConfigurationService } from './configuration.service';
import { AlertService, MessageSeverity } from './alert.service';
import { Dashboard, PaymentType, IcoInfo } from '../models/dashboard.models';
import { Utilities } from './utilities';


@Injectable()
export class DashboardEndpoint extends EndpointFactory {

    private readonly _icoInfoUrl: string = '/api/dashboard/ico_status';
    private readonly _paymentTypesUrl: string = '/api/dashboard/payment_status';


    constructor(http: Http, configurations: ConfigurationService, injector: Injector, alertService: AlertService) {

        super(http, configurations, injector, alertService);
    }

    // public getDashboard(): Observable<Response> {
    //     let dashboard = new Dashboard();
    //     this.getPaymentTypes().subscribe(
    //         info => {
    //             this.alertService.stopLoadingMessage();
    //             dashboard.paymentTypes = info.json() as PaymentType[];
    //         },
    //         error => {
    //             this.alertService.stopLoadingMessage();
    //             let errorMessage = Utilities.findHttpResponseMessage('error_description', error);
    //             this.alertService.showStickyMessage('Server error', errorMessage, MessageSeverity.error);
    //         });
    //     this.getIcoInfo().subscribe(
    //         info => {
    //             this.alertService.stopLoadingMessage();
    //             dashboard.icoInfo = info.json() as IcoInfo;
    //         },
    //         error => {
    //             this.alertService.stopLoadingMessage();
    //             let errorMessage = Utilities.findHttpResponseMessage('error_description', error);
    //             this.alertService.showStickyMessage('Server error', errorMessage, MessageSeverity.error);
    //         });
    //     return dashboard;
    // }

    public getIcoInfo(): Observable<Response> {
        let res = this.http.get(this._icoInfoUrl, this.getAuthHeader())
            .map((response: Response) => {
                return response;
            })
            .catch(error => {

                return this.handleError(error, () => this.getIcoInfo());

            });
        return res;
    }
    public getPaymentTypes(): Observable<Response> {
        let res = this.http.get(this._paymentTypesUrl, this.getAuthHeader())
            .map((response: Response) => {
                return response;
            })
            .catch(error => {

                return this.handleError(error, () => this.getPaymentTypes());

            });
        return res;
    }

}