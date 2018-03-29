import { Injectable } from "@angular/core";
import { ReferralInfo } from "../models/referral/referral-info.model";
import { HttpClient, HttpHeaders } from "@angular/common/http";
import { environment } from "../../environments/environment";
import { Observable } from "rxjs/Observable";
import { ConfigurationService } from "./configuration.service";
import { AuthService } from "./auth.service";
import { BaseService } from './base.service';
import { ReferralCurrencyItem } from "../models/referral/referral-currency-item.model";


@Injectable()
export class ReferralService {
    //todo refactor currencies to config file or smth like this. This list of currencies also in referral component.
    public CURRENCIES = [
        { acronym: 'BTC', name: 'Bitcoin' },
        { acronym: 'ETH', name: 'Etherium' }
    ];
    
    isReferralSystemDisabled: boolean;
    
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
            let items = {};
            for (let curr of this.CURRENCIES) {
                items[curr.acronym] = new ReferralCurrencyItem(
                    response['items'][curr.acronym]['address'],
                    response['items'][curr.acronym]['balance'],
                    response['items'][curr.acronym]['pending'],
                    response['items'][curr.acronym]['transactions']
                );
            }

            return new ReferralInfo(
                response['link'],
                items
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