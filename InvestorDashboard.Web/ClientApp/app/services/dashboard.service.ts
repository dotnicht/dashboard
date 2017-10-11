import { Injectable } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { IPaymentType } from '../models/dashboard.models';

@Injectable()
export class DashboardService {
    public paymentTypes: IPaymentType[];

    constructor() {
        this.Initialize();
    }
    private Initialize() {
        this.paymentTypes = [{ image: '/img/btc-icon.svg', currency: 'BTC', value: '1 BTC = 324.58 DTT' },
        { image: '/img/ltc-icon.svg', currency: 'LTC', value: '1 LTC = 2345.58 DTT' }] as IPaymentType[];
    }
}
