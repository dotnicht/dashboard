export class TokenTransfer {
    amount: number;
    address: string;
    reCaptchaToken: String;
    commision: number;
    constructor(commision: number) {
        this.commision = commision;
        this.amount = 0;
    }
}