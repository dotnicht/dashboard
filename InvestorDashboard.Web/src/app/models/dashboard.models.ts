import { ClientInfo } from "./client-info.model";

export class PaymentType {
    image: string;
    currency: string;
    address: string;
    rate: number;
    faq: string;
    minimum: string;
    eth_to_btc: number;
}

export class IcoInfo {
    tokenName: string;
    totalInvestors: number;
    contractAddress: string;
    isReferralSystemDisabled: boolean;
    // totalUsdInvested: number;
    totalCoins: number;
    totalCoinsBought: number;
    totalCoinsBoughtPercent: number;
    isTokenSaleDisabled: boolean;
    tokenPrice: number;
    kycBonus: number;
    // bonusPercentage: number;
    currencies: { currency: string, value: number, img: string }[];
    constructor() {
        this.totalCoins = 0;
        this.totalInvestors = 0;
        // this.totalUsdInvested = 0;
        this.totalCoinsBought = 0;
        this.totalCoinsBoughtPercent = 0;
        // this.bonusPercentage = 0;
    }
}
export class Dashboard {
    icoInfoModel: IcoInfo = new IcoInfo();
    paymentInfoList: PaymentType[];
    clientInfoModel: ClientInfo = new ClientInfo();
}