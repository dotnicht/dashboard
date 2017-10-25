import { Injectable, Injector } from '@angular/core';
import { Http, Response } from '@angular/http';
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/map';

import { EndpointFactory } from './endpoint-factory.service';
import { ConfigurationService } from './configuration.service';
import { AlertService, MessageSeverity } from './alert.service';
import { Dashboard, PaymentType, IcoInfo } from '../models/dashboard.models';
import { Utilities } from './utilities';
import { ClientInfo } from '../models/client-info.model';

@Injectable()
export class ClientInfoEndpointService extends EndpointFactory {
    public clientInfo: ClientInfo = new ClientInfo();

    private readonly _clientInfoUrl: string = '/api/dashboard/client_info';


    constructor(http: Http, configurations: ConfigurationService, injector: Injector, alertService: AlertService) {

        super(http, configurations, injector, alertService);
    }

    public updateClientInfo() {
        this.getClientInfoEndpoint().subscribe(info => {
            this.clientInfo = info.json() as ClientInfo;
        });
    }
    // protected handleError(error, continuation: () => Observable<any>)  {
    //     return '';
    // }
    private getClientInfoEndpoint(): Observable<Response> {
        let res = this.http.get(this._clientInfoUrl, this.getAuthHeader())
            .map((response: Response) => {
                return response;
            })
            .catch(error => {

                return this.handleError(error, () => this.getClientInfoEndpoint());

            });
        return res;
    }

}
