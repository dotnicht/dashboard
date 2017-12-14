import { Component, ViewChild, EventEmitter, Output } from '@angular/core';
import { TfSettingsComponent } from '../tf_settings.component';

@Component({
    selector: 'app-tf-disable',
    templateUrl: './tf-disable.component.html',
    styleUrls: ['./tf-disable.component.scss']
})
/** TfDisable component*/
export class TfDisableComponent {

    @Output() onSwitchTab = new EventEmitter<number>();

    isLoading = false;


    constructor() {

    }
    switchTab() {
        this.onSwitchTab.emit(0);
    }
    disable() {

    }
}