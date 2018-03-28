import { ReferralCurrencyItem } from "./referral-currency-item.model";

export class ReferralInfo {
    link: string;
    items: {
        BTC: ReferralCurrencyItem;
        ETH: ReferralCurrencyItem;
    };

    constructor (link, items) {
        this.link = link;
        this.items = items;
    }
}