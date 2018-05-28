import { Component, OnInit } from '@angular/core';
import { AppTranslationService } from '../../services/app-translation.service';

@Component({
    selector: 'app-settings',
    templateUrl: './settings.component.html',
    styleUrls: ['./settings.component.scss']
})
/** settings component*/
export class SettingsComponent implements OnInit {
    tabLinks = [
        // { label: 'Profile', link: 'profile' },
        // { label: '2FA', link: '2fa' },
        { label: 'ETH Address', link: 'eth_address' },
        { label: 'Password', link: 'change_password' }
    ];
    tabNavBackground: any = undefined;
    /** settings ctor */
    constructor(private translationService: AppTranslationService) {

    }

    /** Called by Angular after settings component initialized */
    ngOnInit(): void { }
}