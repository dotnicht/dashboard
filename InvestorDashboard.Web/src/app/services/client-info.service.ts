import { Injectable, Injector, OnDestroy, OnInit } from '@angular/core';
import { Http, Response } from '@angular/http';
import { Observable, BehaviorSubject } from 'rxjs';

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
import { DashboardEndpoint } from './dashboard-endpoint.service';
import { HttpClient } from '@angular/common/http';

@Injectable()
export class ClientInfoEndpointService implements OnInit {

    public clientInfo: ClientInfo = new ClientInfo();
    public icoInfo$ = new BehaviorSubject(new IcoInfo());

    public clientInfo$ = new BehaviorSubject(new ClientInfo());


    private readonly _clientInfoUrl: string = environment.host + '/dashboard/client_info';

    constructor(private http: HttpClient, private authService: AuthService, private dashboardEndpoint: DashboardEndpoint) {

    }
    ngOnInit(): void {

    }


    public updateClientInfo() {

        this.getClientInfoEndpoint().subscribe(info => {
            let model = info;
            // model.isTokenSaleDisabled=true;
            model.balance = Math.round(model.balance * 100) / 100;
            model.bonusBalance = Math.round(model.bonusBalance * 100) / 100;
            model.summary = Math.round((model.balance + model.bonusBalance) * 100) / 100;
            this.clientInfo = model;
            this.clientInfo$.next(model);

        });
        this.dashboardEndpoint.getIcoInfo().subscribe(data => {
            this.icoInfo$.next(data as IcoInfo);

        });
    }
    // protected handleError(error, continuation: () => Observable<any>)  {
    //     return '';
    // }
    private getClientInfoEndpoint() {
        let res = this.http.get<ClientInfo>(this._clientInfoUrl, this.authService.getAuthHeader());
        return res;
    }

}
