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
import { MatDialogRef, MatDialog } from '@angular/material';
import { DOCUMENT } from '@angular/platform-browser';

declare var QRCode: any;

@Component({
    selector: 'app-dashboard',
    templateUrl: './dashboard.component.html',
    styleUrls: ['./dashboard.component.scss']
})

export class DashboardComponent implements OnDestroy, OnInit {


    public dashboard: Dashboard = new Dashboard();

    public selectedPaymentType: PaymentType;
    public Question: string;
    public calculateValue = 1;
    public qrLoaded = true;
    public isCopied = false;
    public calculatorFromBtc = true;

    private subscription: any;

    addedQuestionDialogRef: MatDialogRef<AddedQuestionDialogComponent> | null;


    /** dashboard ctor */
    constructor(
        private translationService: AppTranslationService,
        private dashboardService: DashboardEndpoint,
        private clientInfoService: ClientInfoEndpointService,
        private authService: AuthService,
        private dialog: MatDialog,
        @Inject(DOCUMENT) doc: any) {

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
        this.loadData();
        this.subscribeToData();
    }

    public ngOnDestroy(): void {
        if (this.subscription) {
            clearInterval(this.subscription);
        }
    }
    openAddedQuestionDialog() {
        let config = {
            disableClose: true,
            hasBackdrop: false
        };
        this.addedQuestionDialogRef = this.dialog.open(AddedQuestionDialogComponent, config);
        this.addedQuestionDialogRef.afterClosed().subscribe(() => {
            this.Question = undefined;
        });
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
    addQuestion() {
        console.log(this.Question);
        this.dashboardService.addQuestion(this.Question).subscribe(resp => {
            this.openAddedQuestionDialog();
        });
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