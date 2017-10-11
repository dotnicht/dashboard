import { Component, OnInit } from '@angular/core';
import { AppTranslationService } from '../../services/app-translation.service';
import { ClientInfoService } from '../../services/client-info.service';
import { isPlatformBrowser } from '@angular/common';
import { DashboardService } from '../../services/dashboard.service';
import { IPaymentType } from '../../models/dashboard.models';

declare var QRCode: any;

@Component({
    selector: 'app-dashboard',
    templateUrl: './dashboard.component.html',
    styleUrls: ['./dashboard.component.scss']
})

export class DashboardComponent implements OnInit {

    public paymentTypes: IPaymentType[];
    public selectedPaymentType: string;
    /** dashboard ctor */
    constructor(
        private translationService: AppTranslationService,
        private dashboardService: DashboardService,
        private clientInfoService: ClientInfoService) { }

    /** Called by Angular after dashboard component initialized */
    ngOnInit(): void {
        this.paymentTypes = this.dashboardService.paymentTypes;
        if (isPlatformBrowser) {
            this.qrInitialize('http://jindo.dev.naver.com/collie');
        }
    }

    qrInitialize(data: string) {

        let qrCode = new QRCode(document.getElementById('qrCode'), {
            text: data,
            width: 150,
            height: 150
        });
        document.getElementById('qrCode').getElementsByTagName('img')[0].style.display = 'none';
        document.getElementById('qrCode').getElementsByTagName('canvas')[0].style.display = 'block';


    }
    changePayment(payment) {
        this.selectedPaymentType = payment;
    }
}