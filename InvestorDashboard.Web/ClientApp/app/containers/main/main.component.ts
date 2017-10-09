import { Component, OnInit } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { ConfigurationService } from '../../services/configuration.service';
import { LocalStoreManager } from '../../services/local-store-manager.service';

@Component({
    selector: 'app-main',
    templateUrl: './main.component.html',
    styleUrls: ['./main.component.scss']
})

export class MainComponent implements OnInit {
    /** main ctor */
    constructor(storageManager: LocalStoreManager, private authService: AuthService, private configurations: ConfigurationService) {

    }

    ngOnInit(): void { }
}