import { Dictionary } from 'lodash'
import { ReferralTransaction } from './referral-transaction.model';

export class ReferralCurrencyItem {
    address: string;
    previousAddress: string;
    balance: number;
    pending: number;
    transactions: Dictionary<ReferralTransaction>;

    readonlyRefAddress = true;
    isEditModeRefAddress = false;
    addressIsCopied = false;

    constructor(address, balance, pending, transactions) {
        this.address = address;
        this.balance = balance;
        this.pending = pending;
        this.transactions = transactions;
    }
}