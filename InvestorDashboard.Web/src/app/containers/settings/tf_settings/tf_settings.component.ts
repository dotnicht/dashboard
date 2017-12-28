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
class SelectedIndex {
    value = '0';
}
@Component({
    selector: 'app-tf-settings',
    templateUrl: './tf_settings.component.html',
    styleUrls: ['./tf_settings.component.scss']
})
export class TfSettingsComponent implements OnInit {
    tfa: TwoFactorAuthenticationModel = new TwoFactorAuthenticationModel();
    tabIndex = 0;
    // get selectedIndex() {
    //     return this.tabIndex.toString();
    // }
    selectedIndex: SelectedIndex = new SelectedIndex;
    constructor(
        private appTranslationService: AppTranslationService,
        private accountEndpoint: AccountEndpoint) {
    }

    ngOnInit(): void {
        this.updateTabs(0);
    }


    switchTab(index: number) {
        this.selectedIndex.value = index.toString();
    }

    swiperight() {

    }
    swipeleft() {

    }
    updateTabs(index?: number) {
        this.accountEndpoint.TfaDataEndpoint().subscribe(data => {
            this.tfa = data.json() as TwoFactorAuthenticationModel;
            this.tfa.recoveryCodesLeft=0;
            this.switchTab(index);
        });
    }
    selectedTabChange(index: number) {
        this.switchTab(index);
    }
}