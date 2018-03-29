import { Dictionary } from 'lodash'
import { ReferralTransaction } from './referral-transaction.model';

export class ReferralCurrencyItem {
    address: string;
    previousAddress: string;
    balance: number;
    pending: number;
    currAcronym: string;
    currName: string;
    transactions: ReferralTransaction[];

    readonlyRefAddress = true;
    isEditModeRefAddress = false;
    addressIsCopied = false;

    constructor(address, balance, pending, transactions, currency) {
        this.address = address;
        this.balance = balance;
        this.pending = pending;
        this.transactions = transactions;
        this.currAcronym = currency;
    }
}