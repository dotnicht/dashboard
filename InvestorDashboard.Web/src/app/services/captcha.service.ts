
import { Injectable } from '@angular/core';
import { Router, NavigationExtras } from '@angular/router';
import { Http, Headers, RequestOptions, Response, URLSearchParams } from '@angular/http';
import { Observable } from 'rxjs/Observable';
import { Subject } from 'rxjs/Subject';
import 'rxjs/add/operator/map';
import { BaseService } from './base.service';
import { AuthService } from './auth.service';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';


@Injectable()
export class CaptchaEndpoint {

    private readonly _generateGuidUrl: string = (environment.production ? 'https' : 'http') + '://dp-captcha2.azurewebsites.net/api/captchaapi/generateguid';

    constructor(private http: HttpClient, private authService: AuthService) {

    }

    generateGuidEndpoint() {
        const header = new Headers();
        // header.append('Content-Type', 'text/html; charset=utf-8');
        // header.append('Access-Control-Allow-Origin', '*');

        const res = this.http.get(this._generateGuidUrl);
        return res;

    }

}
