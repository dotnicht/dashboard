import { Component, OnInit } from '@angular/core';
import { AppTranslationService } from '../../services/app-translation.service';
import { ClientInfoService } from '../../services/client-info.service';

@Component({
    selector: 'app-dashboard',
    templateUrl: './dashboard.component.html',
    styleUrls: ['./dashboard.component.scss']
})
/** dashboard component*/
export class DashboardComponent implements OnInit {
    /** dashboard ctor */
    constructor(
        private translationService: AppTranslationService, 
        private clientInfoService: ClientInfoService ) { }

    /** Called by Angular after dashboard component initialized */
    ngOnInit(): void { }
}