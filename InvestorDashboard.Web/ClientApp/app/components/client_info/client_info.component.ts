import { Component, OnInit } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { ResizeService } from '../../services/resize.service';
import { ClientInfo } from '../../models/client-info.model';
import { ClientInfoEndpointService } from '../../services/client-info.service';

@Component({
    selector: 'app-client-info',
    templateUrl: './client_info.component.html',
    styleUrls: ['./client_info.component.scss']
})

export class ClientInfoComponent implements OnInit {
    get isMobile(): boolean {
        return this.resizeService.isMobile;
    }

    constructor(private clientInfoService: ClientInfoEndpointService, private authService: AuthService, private resizeService: ResizeService) {

    }

    ngOnInit(): void {
        this.clientInfoService.updateClientInfo();
    }
    get clientInfo() {
        //  return new ClientInfo();
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
