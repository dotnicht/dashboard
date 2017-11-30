import { Component, OnInit, OnDestroy } from '@angular/core';
import { AppTranslationService } from '../../services/app-translation.service';
import { ClientInfoEndpointService } from '../../services/client-info.service';
import { isPlatformBrowser } from '@angular/common';
import { PaymentType, IcoInfo, Dashboard } from '../../models/dashboard.models';
import { DashboardEndpoint } from '../../services/dashboard-endpoint.service';
import { Utilities } from '../../services/utilities';
import { Observable } from 'rxjs/Observable';
import { AnonymousSubscription } from 'rxjs/Subscription';
import 'rxjs/add/observable/timer';
import { AuthService } from '../../services/auth.service';
import { ClientInfo } from '../../models/client-info.model';

declare var QRCode: any;

@Component({
    selector: 'app-dashboard',
    templateUrl: './dashboard.component.html',
    styleUrls: ['./dashboard.component.scss']
})

export class DashboardComponent implements OnDestroy, OnInit {


    public dashboard: Dashboard = new Dashboard();

    public selectedPaymentType: PaymentType;
    public calculateValue = 1;
    public qrLoaded = true;
    public isCopied = false;
    public calculatorFromBtc = true;

    private subscription: any;



    /** dashboard ctor */
    constructor(
        private translationService: AppTranslationService,
        private dashboardService: DashboardEndpoint,
        private clientInfoService: ClientInfoEndpointService,
        private authService: AuthService) {


    }
    ngOnInit(): void {
        this.loadData();
        this.subscribeToData();
    }

    public ngOnDestroy(): void {
        if (this.subscription) {
            clearInterval(this.subscription);
        }
    }
    qrInitialize(data: string) {
        document.getElementById('qrCode').innerHTML = '';
        let qrCode = new QRCode(document.getElementById('qrCode'), {
            text: data,
            width: 150,
            height: 150
        });
        document.getElementById('qrCode').getElementsByTagName('img')[0].style.display = 'none';
        document.getElementById('qrCode').getElementsByTagName('canvas')[0].style.display = 'block';


    }
    toogleCalculator() {
        if (this.calculatorFromBtc) {
            this.calculatorFromBtc = false;
        } else {
            this.calculatorFromBtc = true;
        }
    }
    changePayment(payment: PaymentType) {
        if (!this.dashboard.clientInfoModel.isTokenSaleDisabled && !this.dashboard.icoInfoModel.isTokenSaleDisabled) {
            this.selectedPaymentType = payment;
            this.qrLoaded = false;
            this.isCopied = false;
            setTimeout(() => {
                this.qrLoaded = true;
                if (!this.dashboard.icoInfoModel.isTokenSaleDisabled) {
                    this.qrInitialize(payment.address);
                }


            }, 100);
        }
    }

    loadData() {
        if (this.authService.isLoggedIn) {
            this.dashboardService.getDashboard().subscribe(model => {
                let db = model.json() as Dashboard;
                db.icoInfoModel.totalCoinsBoughtPercent = Math.round((db.icoInfoModel.totalCoinsBought * 100 / db.icoInfoModel.totalCoins) * 100) / 100;
                db.icoInfoModel.totalUsdInvested = Math.round(db.icoInfoModel.totalUsdInvested * 100) / 100;
                db.icoInfoModel.totalCoinsBought = Math.round(db.icoInfoModel.totalCoinsBought * 100) / 100;

                db.paymentInfoList.forEach(element => {
                    element.image = `assets/img/${element.currency}.svg`;
                    element.faq = this.translationService.getTranslation(`dashboard.HTU_${element.currency}`);
                    element.rate = Math.round((element.rate / db.icoInfoModel.tokenPrice) * 100) / 100;
                });

                if (db.paymentInfoList.length > 0) {
                    if (this.selectedPaymentType == undefined) {
                        this.changePayment(db.paymentInfoList[0]);
                    } else {
                        //this.changePayment(db.paymentInfoList[0])
                    }

                }
                db.clientInfoModel = this.clientInfoService.clientInfo;
                this.dashboard = db;
            });
        }
        // this.subscribeToData();
    }
    private subscribeToData(): void {
        this.subscription = setInterval(() => { this.loadData(); }, 30000);
    }


}