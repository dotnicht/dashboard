import { Component, Output, EventEmitter } from '@angular/core';
import { AccountEndpoint } from '../../../../services/account-endpoint.service';

@Component({
    selector: 'app-tf-recovery-key',
    templateUrl: './tf-recovery-key.component.html',
    styleUrls: ['./tf-recovery-key.component.scss']
})
/** TfRecoveryKey component*/
export class TfRecoveryKeyComponent {
    @Output() onUpdateTabs = new EventEmitter<number>();
    constructor(private accountEndpoint: AccountEndpoint) {

    }
    reset() {
        this.accountEndpoint.TfResetEndpoint().subscribe(data => {
            this.onUpdateTabs.emit(0);
        });
    }
}