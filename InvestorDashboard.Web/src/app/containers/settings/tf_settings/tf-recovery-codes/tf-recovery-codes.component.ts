import { Component, OnInit } from '@angular/core';
import { AccountEndpoint } from '../../../../services/account-endpoint.service';

class RecoveryCodes {
    recoveryCodes: string[];
}

@Component({
    selector: 'app-tf-recovery-codes',
    templateUrl: './tf-recovery-codes.component.html',
    styleUrls: ['./tf-recovery-codes.component.scss']
})
/** TfRecoveryCodes component*/
export class TfRecoveryCodesComponent implements OnInit {

    /** TfRecoveryCodes ctor */
    codes: RecoveryCodes = new RecoveryCodes;
    constructor(private accountEndpoint: AccountEndpoint) {

    }
    ngOnInit(): void {
        this.accountEndpoint.TfGetRecoveryCodesEndpoint().subscribe(data => {
            this.codes = data.json() as RecoveryCodes;
        });
    }
}