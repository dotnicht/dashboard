import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs/Observable";
import { Http } from "@angular/http";
import { AuthService } from "./auth.service";
import { environment } from '../../environments/environment';


@Injectable()
export class AdminPanelService { 
    constructor(private authService: AuthService, private http: Http) { 
        // super(authService, http);
    }
    private readonly userTransactionsUrl: string = environment.host + `/dashboard/management`;

    getUserTransactionsByEmail(email: string) {
        let url = this.userTransactionsUrl + `?email=${email}`;
        return this.http.get(url, this.authService.getAuthHeader());
    }

    setTokensToUser(guid: string, amount: number) {
        console.log('guid', guid, amount)
        return this.http.post(this.userTransactionsUrl, {id: guid, amount: amount}, this.authService.getAuthHeader());
    }
}