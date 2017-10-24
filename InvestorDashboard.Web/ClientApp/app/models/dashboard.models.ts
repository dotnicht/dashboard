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
}
export class Dashboard {
    icoInfo: IcoInfo;
    paymentTypes: PaymentType[];
}