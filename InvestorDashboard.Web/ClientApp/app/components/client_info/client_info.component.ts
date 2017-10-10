import { Component, OnInit } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { ResizeService } from '../../services/resize.service';

@Component({
    selector: 'app-client-info',
    templateUrl: './client_info.component.html',
    styleUrls: ['./client_info.component.scss']
})

export class ClientInfoComponent implements OnInit {
    get isMobile(): boolean {
        return this.resizeService.isMobile;
    }
    private balance: number;


    constructor(private authService: AuthService, private resizeService: ResizeService) {
       
    }

    ngOnInit(): void {
        if (this.authService.isLoggedIn) {
            this.balance = 40.05;
        }
     }

    logout() {
        this.authService.logout();
        this.authService.redirectLogoutUser();
    }

    get userName(): string {
        return this.authService.currentUser ? this.authService.currentUser.userName : '';
    }
}
