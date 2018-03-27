import { Component } from '@angular/core';
import { link } from 'fs';
import { ReferralInfo } from '../../models/referral/referral-info.model';
import { ReferralService } from '../../services/referral.service';
import { ReferralCurrencyItem } from '../../models/referral/referral-currency-item.model';


@Component({
    selector: 'app-referral',
    templateUrl: './referral.component.html',
    styleUrls: ['./referral.component.scss']
})
/** referral component*/
export class ReferralComponent {
    /** referral ctor */

    public referralInfo: ReferralInfo;
    public CURRENCIES = [
        { acronym: 'BTC', name: 'Bitcoin' },
        { acronym: 'ETH', name: 'Etherium' }
    ];

    public referralLinkIsCopied: boolean = false;

    constructor(private referralService: ReferralService) {

    }

    ngOnInit() {
        // this.referralInfo.link = "ref_link";

        this.referralService.getReferralInfo().subscribe(data => {
            console.log(data);
            data.items.ETH = new ReferralCurrencyItem(
                'asd',
                2.34,
                0,
                { 1: 1 });
            data.items.BTC.address = 'asdasdasdasd'
            data.items.BTC.readonlyRefAddress = true;
            this.referralInfo = data as ReferralInfo;

        });

    }

    copyToClipboard(copiedElement: string) {
        if (copiedElement.toUpperCase() == 'REF_LINK') {
            this.referralLinkIsCopied = true;
        }
        else {
            this.referralLinkIsCopied = false;
        }

        for (let el of this.CURRENCIES) {
            if (el.acronym == copiedElement.toUpperCase()) {
                this.referralInfo.items[el.acronym].addressIsCopied = true;
            }
            else {
                this.referralInfo.items[el.acronym].addressIsCopied = false;
            }
        }
    }

    private savePreviousAddress(event: any, currencyAcronym: string) {
        this.referralInfo.items[currencyAcronym].currentValue = event.target.value;
        // this.referralInfo.items[currencyAcronym].previosValue = this.referralInfo.items[currencyAcronym].address;
        // this.referralInfo.items[currencyAcronym].address = event;
        console.log(event.target.value, this.referralInfo.items[currencyAcronym].currentValue, this.referralInfo.items[currencyAcronym].address);
    }

    private save(currencyAcronym: string) {
        this.referralService.changeReferralInfo(currencyAcronym, this.referralInfo.items[currencyAcronym].address).subscribe(response => {
            console.log('response', response);
        });
    }

    private edit(currencyAcronym: string) {
        currencyAcronym = currencyAcronym.toUpperCase();
        this.referralInfo.items[currencyAcronym].isEditModeRefAddress = true;
        this.referralInfo.items[currencyAcronym].readonlyRefAddress = !this.referralInfo.items[currencyAcronym].isEditModeRefAddress;
    }

    private cancel(currencyAcronym: string) {
        currencyAcronym = currencyAcronym.toUpperCase();
        this.referralInfo.items[currencyAcronym].isEditModeRefAddress = false;
        this.referralInfo.items[currencyAcronym].readonlyRefAddress = !this.referralInfo.items[currencyAcronym].isEditModeRefAddress;
        
        console.log('cancel', this.referralInfo.items[currencyAcronym].currentValue, this.referralInfo.items[currencyAcronym].address);
        this.referralInfo.items[currencyAcronym].address = this.referralInfo.items[currencyAcronym].address;
    }

    private delete(currencyAcronym: string) {
        this.referralService.changeReferralInfo(currencyAcronym).subscribe(response => {
            console.log('response', response);
        });
    }
}