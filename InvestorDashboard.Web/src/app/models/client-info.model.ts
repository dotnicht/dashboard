export class ClientInfo {
    balance: number;
    bonusBalance: number;
    summary: number;
    isAdmin: boolean;
    // address: string;
    isTokenSaleDisabled: boolean;
    isEligibleForTransfer: boolean;
    thresholdExceeded: boolean;

    constructor() {
        this.balance = 0;
        this.bonusBalance = 0;
    }
}