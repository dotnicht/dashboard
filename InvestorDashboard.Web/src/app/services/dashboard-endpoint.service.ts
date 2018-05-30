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
import { BehaviorSubject, Subject } from 'rxjs';
import { AppTranslationService } from './app-translation.service';
import { HttpClient } from '@angular/common/http';


@Injectable()
export class DashboardEndpoint {

    private readonly _icoInfoUrl: string = environment.host + `/dashboard/ico_status`;
    private readonly _paymentTypesUrl: string = environment.host + `/dashboard/payment_status`;
    private readonly _dashboard: string = environment.host + `/dashboard/full_info`;
    private readonly _addQuestion: string = environment.host + `/dashboard/add_question`;
    private readonly _addTokenTransfer: string = environment.host + `/dashboard/add_token_transfer`;
    private readonly _generateAddresses: string = environment.host + `/dashboard/addresses`;

    private dashboardSource = new BehaviorSubject<Dashboard>(new Dashboard());
    dashboard$ = this.dashboardSource.asObservable();

    private subscription: any;

    constructor(private authService: AuthService, private http: HttpClient, private configurations: ConfigurationService,
        private translationService: AppTranslationService) {

    }

    public getDashboard() {
        // let dashboard = new Dashboard();

        // return dashboard;
        let res = this.http.get(this._dashboard, this.authService.getAuthHeader())
            .map((response) => {
                const db = response as Dashboard;
                db.icoInfoModel.totalCoinsBoughtPercent = Math.round((db.icoInfoModel.totalCoinsBought * 100 / db.icoInfoModel.totalCoins) * 100) / 100;
                // db.icoInfoModel.totalUsdInvested = Math.round(db.icoInfoModel.totalUsdInvested * 100) / 100;
                db.icoInfoModel.totalCoinsBought = Math.round(db.icoInfoModel.totalCoinsBought * 100) / 100;
                db.icoInfoModel.progressPercent = Math.round((db.icoInfoModel.totalCoins / db.icoInfoModel.totalCoinsBought) * 100) / 100;
                db.icoInfoModel.progressTotal = Math.round(db.icoInfoModel.totalCoins * db.icoInfoModel.tokenPrice * 100) / 100;
                db.icoInfoModel.progressTotalBought = Math.round(db.icoInfoModel.totalCoinsBought * db.icoInfoModel.tokenPrice * 100) / 100;

                db.paymentInfoList.forEach(element => {
                    element.image = `assets/img/${element.currency.toLowerCase()}`;
                    element.title = element.currency;
                    element.faq = this.translationService.getTranslation(`dashboard.HTU_${element.currency}`);
                    element.eth_to_btc = Math.round(0.1 / element.rate * 100000) / 100000;
                    element.rate = Math.round((element.rate / db.icoInfoModel.tokenPrice) * 100) / 100;
                    element.minimum = this.translationService.getTranslation(`dashboard.MIN_${element.currency}`);
                    element.type = 0;
                });

                db.icoInfoModel.currencies.forEach(element => {
                    element.img = `assets/img/${element.currency}.svg`;
                });

                db.paymentInfoList.push({
                    currency: '$',
                    image: `assets/img/dolar`, title: 'Wire Transfer', type: 1,
                    rate: Math.round((1 / db.icoInfoModel.tokenPrice) * 100) / 100,
                    minimum: this.translationService.getTranslation(`dashboard.MIN_$`),
                    faq: this.translationService.getTranslation(`dashboard.HTU_$`)
                } as PaymentType);
                // this.etherAddress = db.paymentInfoList.filter(x => x.currency == 'ETH')[0].address;
                // db.clientInfoModel = this.clientInfoService.clientInfo;

                this.dashboardSource.next(db);
                return response;
            });
        return res;
    }

    public getIcoInfo() {

        let res = this.http.get<IcoInfo>(this._icoInfoUrl, this.authService.getAuthHeader());
        return res;
    }
    public getPaymentTypes() {
        let res = this.http.get(this._paymentTypesUrl, this.authService.getAuthHeader());
        return res;
    }

    public addQuestion(text: string) {

        let res = this.http.post(this._addQuestion, { Message: text }, this.authService.getAuthHeader());
        return res;
    }
    public addtokenTransfer(model: TokenTransfer) {

        let res = this.http.post(this._addTokenTransfer, model, this.authService.getAuthHeader());
        return res;
    }
    public generateAddresses() {

        let res = this.http.post(this._generateAddresses, null, this.authService.getAuthHeader());
        return res;
    }
}