import { Component, OnInit } from '@angular/core';
import { AppTranslationService } from '../../../services/app-translation.service';
import { User } from '../../../models/user.model';
import { AuthService } from '../../../services/auth.service';
import { AccountEndpoint } from '../../../services/account-endpoint.service';

class TwoFactorAuthenticationModel {
    hasAuthenticator: boolean;
    recoveryCodesLeft: number;
    is2faEnabled: boolean;
}

@Component({
    selector: 'app-tf-settings',
    templateUrl: './tf_settings.component.html',
    styleUrls: ['./tf_settings.component.scss']
})
/** tfa component*/
export class TfSettingsComponent implements OnInit {
    tfa: TwoFactorAuthenticationModel = new TwoFactorAuthenticationModel();
    tabIndex = 0;
    get selectedIndex() {
        return this.tabIndex.toString();
    }
    constructor(
        private appTranslationService: AppTranslationService,
        private accountEndpoint: AccountEndpoint) {
    }

    /** Called by Angular after tfa component initialized */
    ngOnInit(): void {
        //this.getCurrentUser();
        // this.user.twoFactorEnabled = true;
        this.accountEndpoint.TfaDataEndpoint().subscribe(data => {
            this.tfa = data.json() as TwoFactorAuthenticationModel;
        });
    }


    switchTab(index: number) {
        this.tabIndex = index;
    }

    swiperight() {

    }
    swipeleft() {

    }
}