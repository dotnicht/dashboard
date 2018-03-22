import { Component } from '@angular/core';
import { ReferralInfo } from '../../models/referral-info.model';
import { link } from 'fs';


const CURRENCIES = {
    BTC: 'btc',
    ETH: 'eth'
}

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

    public isEditModeBtcRefAddress: boolean = false;
    public readonlyBtcRefAddress: boolean = !this.isEditModeBtcRefAddress;

    public isEditModeEthRefAddress: boolean = false;
    public readonlyEthRefAddress: boolean = !this.isEditModeEthRefAddress;
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

    private edit(currency: string = null) {
        if (currency.toLowerCase() == CURRENCIES.BTC) {
            this.isEditModeBtcRefAddress = true;
            this.readonlyBtcRefAddress = !this.isEditModeBtcRefAddress;
        }

        //TODO
        if (currency.toLowerCase() == CURRENCIES.ETH) {
            this.isEditModeBtcRefAddress = true;
            this.readonlyBtcRefAddress = !this.isEditModeBtcRefAddress;
        }

        throw `Currency ${currency} is incorrect!`;

    }

    private cancel() {
        this.isEditModeBtcRefAddress = false;
        this.readonlyBtcRefAddress = !this.isEditModeBtcRefAddress;
    }
}