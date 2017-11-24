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
        { label: 'Profile', link: 'profile' },
        //{ label: 'Two-factor authentication', link: '2fa' },
        { label: 'Password', link: 'change_password' }
    ];
    tabNavBackground: any = undefined;
    /** settings ctor */
    constructor(private translationService: AppTranslationService) {
   
     }

    /** Called by Angular after settings component initialized */
    ngOnInit(): void { }
}