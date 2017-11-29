import { Component, OnInit, ViewChild, ElementRef, OnDestroy } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { ResizeService } from '../../services/resize.service';
import { ClientInfo } from '../../models/client-info.model';
import { ClientInfoEndpointService } from '../../services/client-info.service';
import { AppComponent } from '../../app.component';

@Component({
    selector: 'app-client-info',
    templateUrl: './client_info.component.html',
    styleUrls: ['./client_info.component.scss']
})

export class ClientInfoComponent implements OnDestroy, OnInit {

    @ViewChild('start') public sideNav: ElementRef;


    private clientInfoSubscription: any;
    get isTab(): boolean {
        return this.resizeService.isTab;
    }

    constructor(private clientInfoService: ClientInfoEndpointService,
        private authService: AuthService,
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
