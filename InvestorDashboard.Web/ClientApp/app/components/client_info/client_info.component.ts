import { Component, OnInit, ViewChild, ElementRef } from '@angular/core';
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

export class ClientInfoComponent implements OnInit {

    @ViewChild('start') public sideNav: ElementRef;
    
    get isTab(): boolean {
        return this.resizeService.isTab;
    }

    constructor(private clientInfoService: ClientInfoEndpointService, private authService: AuthService, private resizeService: ResizeService) {

    }

    ngOnInit(): void {
        if (this.authService.isLoggedIn) {

            this.clientInfoService.updateClientInfo();
        }

    }
    get clientInfo() {
        //  return new ClientInfo();
        console.log(this.clientInfoService.clientInfo)
        return this.clientInfoService.clientInfo;
    }
    logout() {
        this.authService.logout();
        this.authService.redirectLogoutUser();
    }

    get userName(): string {
        return this.authService.currentUser ? this.authService.currentUser.userName : '';
    }
}
