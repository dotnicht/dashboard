import { Component, OnInit, ViewChild, ElementRef, OnDestroy, AfterViewInit } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { ResizeService } from '../../services/resize.service';
import { ClientInfo } from '../../models/client-info.model';
import { ClientInfoEndpointService } from '../../services/client-info.service';
import { AppComponent } from '../../app.component';
import { DashboardEndpoint } from '../../services/dashboard-endpoint.service';
import { Dashboard } from '../../models/dashboard.models';

@Component({
    selector: 'app-client-info',
    templateUrl: './client_info.component.html',
    styleUrls: ['./client_info.component.scss']
})

export class ClientInfoComponent implements OnDestroy, OnInit {

    @ViewChild('start') public sideNav: ElementRef;

    clientInfo: ClientInfo = new ClientInfo();
    currency: string;

    observableList = [];

    get isTab(): boolean {
        return this.resizeService.isTab;
    }

    constructor(private clientInfoService: ClientInfoEndpointService,
        private authService: AuthService,
        private dashboardService: DashboardEndpoint,
        private resizeService: ResizeService) {
    }
    // get clientInfo() {
    //     return this.clientInfoService.clientInfo;
    // }
    ngOnInit(): void {
        // this.refreshData();

        this.observableList.push(this.dashboardService.dashboard$.subscribe(model => {
            const db = model;
            this.clientInfo = model.clientInfoModel;
            this.clientInfo.balance = Math.round(this.clientInfo.balance * 100) / 100;
            this.clientInfo.bonusBalance = Math.round(this.clientInfo.bonusBalance * 100) / 100;
            this.clientInfo.summary = Math.round((this.clientInfo.balance + this.clientInfo.bonusBalance) * 100) / 100;
            this.currency = db.icoInfoModel.tokenName;
        }));

    }
    ngOnDestroy(): void {
        this.observableList.map((el) => {
            el.unsubscribe();
        });
    }

    logout() {
        this.authService.logout();
        // this.authService.redirectLogoutUser();
    }

    get userName(): string {
        return this.authService.currentUser ? this.authService.currentUser.email : '';
    }


}
