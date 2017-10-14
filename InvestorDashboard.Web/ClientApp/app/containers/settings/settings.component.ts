import { Component, OnInit } from '@angular/core';

@Component({
    selector: 'app-settings',
    templateUrl: './settings.component.html',
    styleUrls: ['./settings.component.scss']
})
/** settings component*/
export class SettingsComponent implements OnInit {
    tabLinks = [
        { label: 'Profile', link: 'profile' },
        { label: 'Two-factor authentication', link: '2fa' },
        { label: 'Password', link: 'password' }
    ];

    /** settings ctor */
    constructor() { }

    /** Called by Angular after settings component initialized */
    ngOnInit(): void { }
}