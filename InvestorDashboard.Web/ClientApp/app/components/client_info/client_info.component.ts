import { Component, OnInit } from '@angular/core';
import { AuthService } from '../../services/auth.service';

@Component({
    selector: 'app-client-info',
    templateUrl: './client_info.component.html',
    styleUrls: ['./client_info.component.scss']
})

export class ClientInfoComponent implements OnInit {
    private isMobile: boolean = false;
    // private clientImg: any= require('../../assets/images/people.svg');
    private menuItems = [{
        title: 'settings',
        imgName: 'settings_applications',
        route: '/',
        click: ''
    },
    {
        title: 'logout',
        imgName: 'exit_to_app',
        route: '/',
        click: 'logout()'
    }];

    constructor(private authService: AuthService) { }

    ngOnInit(): void { }

    logout() {
        this.authService.logout();
        this.authService.redirectLogoutUser();
    }

    get userName(): string {
        return this.authService.currentUser ? this.authService.currentUser.userName : '';
    }
}