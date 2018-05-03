import { ReferralCurrencyItem } from "./referral-currency-item.model";

export class ReferralInfo {
    link: string;
    items: ReferralCurrencyItem[];
    count: number;
    tokens: number;

    constructor (link, items, count, tokens) {
        this.link = link;
        this.items = items;
        this.count = count;
        this.tokens = tokens;
    }
}