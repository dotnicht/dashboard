export class PaymentType {
    image: string;
    currency: string;
    address: string;
    rate: number;
    faq: string;
}

export class IcoInfo {
    totalInvestors: number;
    totalUsdInvested: number;
    totalCoins: number;
    totalCoinsBought: number;
    totalCoinsBoughtPercent: number;
    constructor() {
        this.totalCoins = 0;
        this.totalInvestors = 0;
        this.totalUsdInvested = 0;
        this.totalCoinsBought = 0;
        this.totalCoinsBoughtPercent = 0;
    }
}
export class Dashboard {
    icoInfo: IcoInfo;
    paymentTypes: PaymentType[];
}