export class TokenTransfer {
    amount = 1;
    address: string;
    reCaptchaToken: String;
    commision: number;
    constructor(commision: number) {
        this.commision = commision;
    }
}