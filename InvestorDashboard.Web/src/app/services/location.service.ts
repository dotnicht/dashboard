import { Injectable } from '@angular/core';
import { Observable } from 'rxjs/Observable';
import { Http } from '@angular/http';
import { LocationModel } from '../models/location.model';
import { environment } from '../../environments/environment';

@Injectable()
export class CurrentLocationService {
    private readonly _getIpInfoUrl = (environment.production ? 'https' : 'http') + '://ipinfo.io';

    constructor(private _http: Http) { }

    getCurrentIpLocation(): Observable<LocationModel> {
        return this._http.get(this._getIpInfoUrl)
            .map(response => response.json())
            .catch(error => {
                console.log(error);
                return Observable.throw(error.json());
            });
    }
}