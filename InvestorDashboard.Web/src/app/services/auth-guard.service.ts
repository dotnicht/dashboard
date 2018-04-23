import { Injectable, OnInit } from '@angular/core';
import {
    CanActivate, Router, ActivatedRouteSnapshot, RouterStateSnapshot,
    CanActivateChild, NavigationExtras, CanLoad, Route
} from '@angular/router';
import { AuthService } from './auth.service';
import { ReferralService } from './referral.service';
import { IcoInfo } from '../models/dashboard.models';
import { ClientInfoEndpointService } from './client-info.service';


@Injectable()
export class AuthGuard implements CanActivate, CanActivateChild, CanLoad {
    private isReferralSystemDisabled: boolean;

    constructor(
        private authService: AuthService,
        private router: Router,
        private referralService: ReferralService,
        private clientInfoEndpointService: ClientInfoEndpointService
    ) {
        this.clientInfoEndpointService.icoInfo$.subscribe(data => {
            this.isReferralSystemDisabled = data.isReferralSystemDisabled;
        })
    }


    canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean {
        let url: string = state.url;
        return this.checkLogin(url);
    }

    canActivateChild(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean {
        return this.canActivate(route, state);
    }

    canLoad(route: Route): boolean {

        let url = `/${route.path}`;
        return this.checkLogin(url);
    }

    checkLogin(url: string): boolean {
        if (this.authService.isLoggedIn) {
            if (url == '/referral' && this.isReferralSystemDisabled) {
                this.router.navigate(['/dashboard']);
                return false;
            }
            return true;
        }
        
        this.authService.loginRedirectUrl = url;
        this.router.navigate(['/login']);

        return false;
    }
}