export class PaymentType {
    image: string;
    currency: string;
    address: string;
    rate: number;
}

export class IcoInfo {
    totalInvestors: number;
    totalUsd: number;
    totalCoins: number;
    constructor() {
        this.totalCoins = 0;
        this.totalInvestors = 0;
        this.totalUsd = 0;
    }
}
export class Dashboard {
    icoInfo: IcoInfo;
    paymentTypes: PaymentType[];
}