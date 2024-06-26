import { Injectable } from "@angular/core";
import { ReferralInfo } from "../models/referral/referral-info.model";
import { HttpClient, HttpHeaders } from "@angular/common/http";
import { environment } from "../../environments/environment";
import { Observable } from "rxjs/Observable";
import { ConfigurationService } from "./configuration.service";
import { AuthService } from "./auth.service";
import { BaseService } from './base.service';
import { ReferralCurrencyItem } from "../models/referral/referral-currency-item.model";
import { Subject } from "rxjs";


@Injectable()
export class ReferralService {
    queryParams = {};
    startUrl: string = '';

    private referralInfoUrl = environment.host + '/dashboard/referral'

    constructor(private http: HttpClient, private authService: AuthService) {
    }
    getReferralInfo(): Observable<ReferralInfo> {
        return this.http.get<ReferralInfo>(this.referralInfoUrl, this.authService.getAuthHeader())
            .map((response) => {
                let items = [];
                for (let item of response['items']) {
                    items.push(new ReferralCurrencyItem(
                        item['address'],
                        item['balance'],
                        item['pending'],
                        item['transactions'],
                        item['currency']
                    ));
                }

                return new ReferralInfo(
                    response['link'],
                    items,
                    response['count'],
                    response['tokens'],
                );
            });
    }

    changeReferralInfo(currencyAcronym: string, refAddress: string = null) {

        let postData = {
            currency: currencyAcronym,
            address: refAddress
        };

        const res = this.http.post(this.referralInfoUrl, JSON.stringify(postData), this.authService.getAuthHeader());
        return res;
    }




}