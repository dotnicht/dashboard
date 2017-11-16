import { Injectable, Injector, OnDestroy, OnInit } from '@angular/core';
import { Http, Response } from '@angular/http';
import { Observable } from 'rxjs';

import 'rxjs/add/operator/map';
import 'rxjs/add/operator/first';

import { ConfigurationService } from './configuration.service';
import { Dashboard, PaymentType, IcoInfo } from '../models/dashboard.models';
import { Utilities } from './utilities';
import { ClientInfo } from '../models/client-info.model';
import { BaseService } from './base.service';
import { AuthService } from './auth.service';
import { environment } from '../../environments/environment';
import { Subscription } from 'rxjs/Subscription';

@Injectable()
export class ClientInfoEndpointService extends BaseService implements  OnInit {

    public clientInfo: ClientInfo = new ClientInfo();

    

    private readonly _clientInfoUrl: string = environment.host + '/dashboard/client_info';

    constructor(http: Http, authService: AuthService) {

        super(authService, http);
      
    }
    ngOnInit(): void {

    }


    public updateClientInfo() {

        this.getClientInfoEndpoint().subscribe(info => {
            let model = info.json() as ClientInfo;
            // model.isTokenSaleDisabled=true;
            model.balance = Math.round(model.balance * 100) / 100;
            model.bonusBalance = Math.round(model.bonusBalance * 100) / 100;
            this.clientInfo = model;

        });
    }
    // protected handleError(error, continuation: () => Observable<any>)  {
    //     return '';
    // }
    private getClientInfoEndpoint(): Observable<Response> {
        let res = this.http.get(this._clientInfoUrl, this.authService.getAuthHeader())
            .map((response: Response) => {
                return response;
            })
            .catch(error => {

                return this.handleError(error, () => this.authService.refreshLogin());

            });
        return res;
    }
   
}
