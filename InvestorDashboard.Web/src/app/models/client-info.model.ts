export class ClientInfo {
    balance: number;
    bonusBalance: number;
    summary: number;
    address: string;
    isTokenSaleDisabled: boolean;
    isEligibleForTransfer: boolean;

    constructor() {
        this.balance = 0;
        this.bonusBalance = 0;
    }
}