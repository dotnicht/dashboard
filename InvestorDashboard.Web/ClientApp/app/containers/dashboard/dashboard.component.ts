import { Component, OnInit, OnDestroy } from '@angular/core';
import { AppTranslationService } from '../../services/app-translation.service';
import { ClientInfoEndpointService } from '../../services/client-info.service';
import { isPlatformBrowser } from '@angular/common';
import { PaymentType, IcoInfo, Dashboard } from '../../models/dashboard.models';
import { DashboardEndpoint } from '../../services/dashboard-endpoint.service';
import { AlertService, MessageSeverity } from '../../services/alert.service';
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

export class DashboardComponent implements OnInit, OnDestroy {

    public dashboard: Dashboard = new Dashboard();

    public paymentTypes: PaymentType[];
    public icoInfo: IcoInfo = new IcoInfo();

    public selectedPaymentType: string;

    public qrLoaded: boolean = true;

    private refreshSubscription: AnonymousSubscription;
    private icoInfoSubscription: AnonymousSubscription;
    private paymentTypesSubscription: AnonymousSubscription;
    /** dashboard ctor */
    constructor(
        private translationService: AppTranslationService,
        private dashboardService: DashboardEndpoint,
        private clientInfoService: ClientInfoEndpointService,
        private alertService: AlertService,
        private authService: AuthService) {

    }
    get clientInfo() {
        //return new ClientInfo();
         return this.clientInfoService.clientInfo;
    }
    /** Called by Angular after dashboard component initialized */
    ngOnInit(): void {
        this.alertService.startLoadingMessage();
        if (this.authService.isLoggedIn) {
            this.loadData();
        }

        this.alertService.stopLoadingMessage();
        // if (isPlatformBrowser) {
        //     this.qrInitialize('http://jindo.dev.naver.com/collie');
        // }
    }
    public ngOnDestroy(): void {
        if (this.paymentTypesSubscription) {
            this.paymentTypesSubscription.unsubscribe();
        }
        if (this.icoInfoSubscription) {
            this.icoInfoSubscription.unsubscribe();
        }
        if (this.refreshSubscription) {
            this.refreshSubscription.unsubscribe();
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
    changePayment(payment) {
        this.qrLoaded = false;
        this.selectedPaymentType = payment;
        this.alertService.startLoadingMessage();
        setTimeout(() => {
            this.qrLoaded = true;
            this.qrInitialize(payment);
            this.alertService.stopLoadingMessage();

        }, 100);

    }

    loadData() {

        this.icoInfoSubscription = this.dashboardService.getIcoInfo().subscribe(info => {
            this.icoInfo = info.json() as IcoInfo;
            this.dashboard.icoInfo = info.json() as IcoInfo;
        });
        this.paymentTypesSubscription = this.dashboardService.getPaymentTypes().subscribe(info => {
            this.dashboard.paymentTypes = info.json() as PaymentType[];
            this.paymentTypes = info.json() as PaymentType[];
        });
        // this.subscribeToData();
    }
    private subscribeToData(): void {
        this.refreshSubscription = Observable.timer(30000).first().subscribe(() => this.loadData());
    }


}