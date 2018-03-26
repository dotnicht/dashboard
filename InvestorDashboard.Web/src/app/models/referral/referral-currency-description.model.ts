export class ReferralCurrencyDescription {
    address: string;
    balance: number;
    transactions: string[];

    readonlyRefAddress: boolean;
    isEditModeRefAddress: boolean;
    addressIsCopied: boolean;

    constructor (address, balance, transactions) {
        this.address = address;
        this.balance = balance;
        this.transactions = transactions;
    }
}