export class ClientInfo {
    balance: number;
    bonusBalance: number;
    summary: number;
    // address: string;
    isTokenSaleDisabled: boolean;
    isEligibleForTransfer: boolean;
    thresholdExceeded: boolean;
    isInvestor: boolean;
    tokensAvailableForTransfer: number;

    constructor() {
        this.balance = 0;
        this.bonusBalance = 0;
    }
}