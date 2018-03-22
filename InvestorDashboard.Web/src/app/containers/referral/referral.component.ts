import { Component } from '@angular/core';
import { ReferralInfo } from '../../models/referral-info.model';
import { link } from 'fs';

@Component({
    selector: 'app-referral',
    templateUrl: './referral.component.html',
    styleUrls: ['./referral.component.scss']
})
/** referral component*/
export class ReferralComponent {
    /** referral ctor */
    public referralInfo: ReferralInfo;

    public referralLinkIsCopied: boolean = false;
    public btcAddressIsCopied: boolean = false;
    public ethAddressIsCopied: boolean = false;

    constructor() {

    }

    ngOnInit() {
        this.referralInfo = new ReferralInfo();

        this.referralInfo.link = "ref_link";

        this.referralInfo.btc_address = "btc_address";
        this.referralInfo.eth_address = "eth_address";
        
        this.referralInfo.btc_balance = 0.0174;
        this.referralInfo.eth_balance = 2.34;

        this.referralInfo.btc_transactions = ["btc_transaction1", "btc_transaction2", "btc_transaction3"];
        this.referralInfo.eth_transactions = ["eth_transaction1", "eth_transaction2", "eth_transaction3"];
    }
}