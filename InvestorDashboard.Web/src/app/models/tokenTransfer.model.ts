export class TokenTransfer {
    amount = 0;
    address: string;
    reCaptchaToken: String;
    commision: number;
    constructor(commision: number) {
        this.commision = commision;
    }
}