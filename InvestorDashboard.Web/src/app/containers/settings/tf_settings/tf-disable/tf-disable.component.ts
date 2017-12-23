import { Component, ViewChild, EventEmitter, Output } from '@angular/core';
import { TfSettingsComponent } from '../tf_settings.component';
import { AccountEndpoint } from '../../../../services/account-endpoint.service';
import { Utilities } from '../../../../services/utilities';

@Component({
    selector: 'app-tf-disable',
    templateUrl: './tf-disable.component.html',
    styleUrls: ['./tf-disable.component.scss']
})
/** TfDisable component*/
export class TfDisableComponent {

    @Output() onSwitchTab = new EventEmitter<number>();
    @Output() onUpdateTabs = new EventEmitter();
    errors = '';


    isLoading = false;
    switchTab() {
        this.onSwitchTab.emit(1);
    }

    constructor(private accountEndpoint: AccountEndpoint) {

    }

    disable() {
        this.accountEndpoint.TfDisableEndpoint().subscribe(data => {
            this.onUpdateTabs.emit(0);
        }, errors => {
            this.errors = Utilities.findHttpResponseMessage('error_description', errors);
        });
    }
}