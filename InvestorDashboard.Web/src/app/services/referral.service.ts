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
        let httpOptions = {
            headers: new HttpHeaders({
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${this.authService.accessToken}`,
                'Accept': `application/vnd.iman.v1+json, application/json, text/plain, */*`,
                'App-Version': ConfigurationService.appVersion
            })
        };

        return this.http.get<ReferralInfo>(this.referralInfoUrl, httpOptions)
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

    changeReferralInfo(currencyAcronym: string, refAddress: string = null): Observable<Response> {
        let httpOptions = {
            headers: new HttpHeaders({
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${this.authService.accessToken}`,
                'Accept': `application/vnd.iman.v1+json, application/json, text/plain, */*`,
                'App-Version': ConfigurationService.appVersion
            })
        };

        let postData = {
            currency: currencyAcronym,
            address: refAddress
        };

        const res = this.http.post(this.referralInfoUrl, JSON.stringify(postData), httpOptions)
            .map((response: Response) => {
                return response;
            })
        // .catch(error => {
        //     //return this.handleError(error, () => this.changeReferralInfo(form));

        // });
        return res;
    }

    


}