import { Component } from '@angular/core';
import { link } from 'fs';
import { ReferralInfo } from '../../models/referral/referral-info.model';
import { ReferralCurrencyDescription } from '../../models/referral/referral-currency-description.model';


const CURRENCIES = {
    BTC: 'BTC',
    ETH: 'ETH'
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
        this.referralInfo[CURRENCIES.BTC] = new ReferralCurrencyDescription(
            "btc_address",
            0.0174,
            ["btc_transaction1", "btc_transaction2", "btc_transaction3"]
        );

        this.referralInfo[CURRENCIES.ETH] = new ReferralCurrencyDescription(
            "eth_address",
            2.34,
            ["eth_transaction1", "eth_transaction2", "eth_transaction3"]
        );

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