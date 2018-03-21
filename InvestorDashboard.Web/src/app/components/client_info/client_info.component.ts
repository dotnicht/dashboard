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

export class ClientInfoComponent implements OnDestroy, OnInit, AfterViewInit {

    @ViewChild('start') public sideNav: ElementRef;

    currency: string;

    private clientInfoSubscription: any;

    get isTab(): boolean {
        return this.resizeService.isTab;
    }

    constructor(private clientInfoService: ClientInfoEndpointService,
        private authService: AuthService,
        private dashboardService: DashboardEndpoint,
        private resizeService: ResizeService) {
    }
    get clientInfo() {
        return this.clientInfoService.clientInfo;
    }
    ngOnInit(): void {
        this.refreshData();
        this.subscribeToClientInfoData();

    }
    ngOnDestroy() {
        clearInterval(this.clientInfoSubscription);
    }
    ngAfterViewInit() {
        this.dashboardService.getDashboard().subscribe(data => {
            const db = data.json() as Dashboard;
            this.currency = db.icoInfoModel.tokenName;
        });
    }
    logout() {
        this.authService.logout();
        this.authService.redirectLogoutUser();
    }

    get userName(): string {
        return this.authService.currentUser ? this.authService.currentUser.userName : '';
    }
    private refreshData() {
        if (this.authService.isLoggedIn && !this.authService.isSessionExpired) {
            //    console.log(this.clientInfo);
            this.clientInfoService.updateClientInfo();
        }
    }
    private subscribeToClientInfoData(): void {
        this.clientInfoSubscription = setInterval(() => { this.refreshData(); }, 30000);
    }
}
