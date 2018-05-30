import { Component, OnInit, OnDestroy, Inject } from '@angular/core';
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
import { MatDialogRef, MatDialog, MAT_DIALOG_DATA } from '@angular/material';
import { DOCUMENT, DomSanitizer } from '@angular/platform-browser';

declare var QRCode: any;

export class Timer {
    days = 0;
    hours = 0;
    minutes = 0;
    seconds = 0;
}

@Component({
    selector: 'app-dashboard',
    templateUrl: './dashboard.component.html',
    styleUrls: ['./dashboard.component.scss']
})

export class DashboardComponent implements OnDestroy, OnInit {


    public dashboard: Dashboard = new Dashboard();

    isGenProcesing = false;
    public selectedPaymentType: PaymentType;
    public etherAddress: string;
    public Question: string;
    public calculateValue = 1;
    public qrLoaded = true;
    public isCopied = false;
    public calculatorFromBtc = true;
    public selectedVideo: any;

    timer = '00 : 00 : 00 : 00';
    timerInterval: any;

    private observableList = [];

    addedQuestionDialogRef: MatDialogRef<AddedQuestionDialogComponent> | null;
    eurDialogRef: MatDialogRef<EurDialogComponent> | null;
    usdDialogRef: MatDialogRef<UsdDialogComponent> | null;


    qrDialogRef: MatDialogRef<QrDialogComponent> | null;


    /** dashboard ctor */
    constructor(
        private translationService: AppTranslationService,
        private dashboardService: DashboardEndpoint,
        private clientInfoService: ClientInfoEndpointService,
        private sanitizer: DomSanitizer,
        private authService: AuthService,
        private dialog: MatDialog,
        @Inject(DOCUMENT) doc: any
    ) {

        dialog.afterOpen.subscribe(() => {
            if (!doc.body.classList.contains('no-scroll')) {
                doc.body.classList.add('no-scroll');
            }
        });
        dialog.afterAllClosed.subscribe(() => {
            doc.body.classList.remove('no-scroll');
        });
    }
    ngOnInit(): void {
        // this.loadData();
        this.chooseVideo();



        this.observableList.push(this.dashboardService.dashboard$.subscribe(model => {
            let db = model;
            if (db.paymentInfoList) {
                if (db.paymentInfoList.length > 0) {
                    if (this.selectedPaymentType == undefined) {
                        this.changePayment(db.paymentInfoList[0]);
                    }

                }
               
                this.dashboard = db;
                this.timerInterval = setInterval(() => { this.updateTimer(); }, 1000);
            }
        }));
    }

    ngOnDestroy(): void {
        clearInterval(this.timerInterval);
        this.observableList.map((el) => {
            el.unsubscribe();
        });
    }
    openAddedQuestionDialog() {
        const config = {
            disableClose: true,
            hasBackdrop: false
        };
        this.addedQuestionDialogRef = this.dialog.open(AddedQuestionDialogComponent, config);
        this.addedQuestionDialogRef.afterClosed().subscribe(() => {
            this.Question = undefined;
        });
    }
    openQrDialog() {
        const config = {
            hasBackdrop: true,
            data: this.selectedPaymentType.address
        };
        this.qrDialogRef = this.dialog.open(QrDialogComponent, config);
    }
    openEurDialog() {
        const config = {
            hasBackdrop: true
        };
        this.eurDialogRef = this.dialog.open(EurDialogComponent, config);
    }
    openUsdDialog() {
        const config = {
            hasBackdrop: true
        };
        this.usdDialogRef = this.dialog.open(UsdDialogComponent, config);
    }
    generateAddress() {
        this.isGenProcesing = true;
        this.dashboardService.generateAddresses().subscribe(data => {
            this.selectedPaymentType = null;
            this.isGenProcesing = false;
            this.loadData();
        });
    }
    updateTimer() {
        const endDate = new Date(this.dashboard.icoInfoModel.bonusValidUntil).getTime();

        const d = new Date();
        const localTime = d.getTime();
        const localOffset = d.getTimezoneOffset() * 60000;
        const utc = localTime + localOffset;

        const today = new Date(utc).getTime();
        const day = Math.floor((endDate - today) / (24 * 60 * 60 * 1000)).toString();
        const hour = Math.floor(((endDate - today) % (24 * 60 * 60 * 1000)) / (60 * 60 * 1000)).toString();
        const min = (Math.floor(((endDate - today) % (24 * 60 * 60 * 1000)) / (60 * 1000)) % 60).toString();
        const sec = (Math.floor(((endDate - today) % (24 * 60 * 60 * 1000)) / 1000) % 60 % 60).toString();
        this.timer = `${day.length == 1 ? '0' : ''}${day} : ${hour.length == 1 ? '0' : ''}${hour} : ${min.length == 1 ? '0' : ''}${min} : ${sec.length == 1 ? '0' : ''}${sec}`;
        // this.timer = { days: day, hours: hour, minutes: min, seconds: sec } as Timer;
    }
    qrInitialize(data: string) {
        if (document.getElementById('qrCode')) {
            document.getElementById('qrCode').innerHTML = '';
            let qrCode = new QRCode(document.getElementById('qrCode'), {
                text: data,
                width: 150,
                height: 150
            });
            document.getElementById('qrCode').getElementsByTagName('img')[0].style.display = 'none';
            document.getElementById('qrCode').getElementsByTagName('canvas')[0].style.display = 'block';

        }
    }

    chooseVideo() {
        const list = [
            'https://www.youtube.com/embed/YgY6o1LNlq4',
            'https://www.youtube.com/embed/IF8sihjNO_M',
            'https://www.youtube.com/embed/EuJCTbdF54s'
        ];
        const index = Math.floor((Math.random() * list.length));

        this.selectedVideo = this.sanitizer.bypassSecurityTrustResourceUrl(list[index]);

    }

    toogleCalculator() {
        if (this.calculatorFromBtc) {
            this.calculatorFromBtc = false;
        } else {
            this.calculatorFromBtc = true;
        }
    }
    changePayment(payment: PaymentType) {
        if (!this.dashboard.icoInfoModel.isTokenSaleDisabled) {
            if (!this.dashboard.clientInfoModel.isTokenSaleDisabled) {
                this.selectedPaymentType = payment;
                this.qrLoaded = false;
                this.isCopied = false;
                setTimeout(() => {
                    this.qrLoaded = true;
                    //if (!this.dashboard.icoInfoModel.isTokenSaleDisabled) {
                    this.qrInitialize(payment.address);
                    //}


                }, 100);
            }
        }
    }
    addQuestion() {
        console.log(this.Question);
        this.dashboardService.addQuestion(this.Question).subscribe(resp => {
            this.openAddedQuestionDialog();
        });
    }
    loadData() {
        if (this.authService.isLoggedIn) {
            this.dashboardService.getDashboard().subscribe();
        }
    }



}
@Component({
    selector: 'app-added-question-dialog',
    template: `

    <h2 mat-dialog-title><mat-icon>check_circle</mat-icon>Question added successfully</h2>
        <button style="float: right" (click)="close()" mat-raised-button tabindex="1">
            <span>{{'buttons.Close' | translate}}</span>
        </button>
    `
})
export class AddedQuestionDialogComponent {

    email: string;
    constructor(public dialogRef: MatDialogRef<AddedQuestionDialogComponent>) {

    }
    close() {
        this.dialogRef.close();
    }

}

@Component({
    selector: 'app-qr-dialog',
    template: `
    <div id="qrCode" class="qrCode"></div>
    `
})
export class QrDialogComponent {
    qrData: string;
    constructor(public dialogRef: MatDialogRef<QrDialogComponent>,
        @Inject(MAT_DIALOG_DATA) public data: any) {
        this.qrData = data;
    }
    ngOnInit() {
        this.qrInitialize(this.qrData);
    }
    close() {
        this.dialogRef.close();
    }
    qrInitialize(data: string) {
        if (document.getElementById('qrCode')) {
            document.getElementById('qrCode').innerHTML = '';
            let qrCode = new QRCode(document.getElementById('qrCode'), {
                text: data,
                width: 300,
                height: 300
            });
            document.getElementById('qrCode').getElementsByTagName('img')[0].style.display = 'none';
            document.getElementById('qrCode').getElementsByTagName('canvas')[0].style.display = 'block';
        }
    }
}
@Component({
    selector: 'app-eur-dialog',
    templateUrl: './eur.component.html',
})
export class EurDialogComponent implements OnInit {
    qrData: string;
    constructor(public dialogRef: MatDialogRef<EurDialogComponent>,
        @Inject(MAT_DIALOG_DATA) public data: any) {
        this.qrData = data;
    }
    ngOnInit() {
    }
    close() {
        this.dialogRef.close();
    }
}

@Component({
    selector: 'app-usd-dialog',
    templateUrl: './usd.component.html',
})
export class UsdDialogComponent implements OnInit {
    qrData: string;
    constructor(public dialogRef: MatDialogRef<UsdDialogComponent>,
        @Inject(MAT_DIALOG_DATA) public data: any) {
        this.qrData = data;
    }
    ngOnInit() {
    }
    close() {
        this.dialogRef.close();
    }
}
