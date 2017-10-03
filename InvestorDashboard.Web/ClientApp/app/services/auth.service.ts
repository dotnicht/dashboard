import { Injectable } from '@angular/core';
import { Http } from '@angular/http';
import { Response, Headers } from '@angular/http';
import { Observable } from 'rxjs/Observable';
import { UserLogin } from '../models/User';
import 'rxjs/add/operator/map';
import 'rxjs/add/operator/catch';
import 'rxjs/add/observable/throw';

@Injectable()
export class AuthService {
  constructor(private http: Http) { }

  send(user: UserLogin) {

    let headers = new Headers();
    headers.append('Content-Type', 'application/x-www-form-urlencoded');
    // headers.append('Content-Type', 'application/json');

    user.grant_type = 'password';
    user.scope = 'openid email phone profile offline_access roles';
    user.resource = window.location.origin;
    
    let data = JSON.stringify(user);


    let searchParams = new URLSearchParams();
    searchParams.append('username', user.email);
    searchParams.append('password', user.password);
    searchParams.append('client_id', 'ID');
    searchParams.append('client_secret', '901564A5-E7FE-42CB-B10D-61EF6A8F3654');
    searchParams.append('grant_type', 'password');
    searchParams.append('scope', 'openid email phone profile offline_access roles');
    searchParams.append('resource', window.location.origin);

    let requestBody = searchParams.toString();

    return this.http.post('/connect/token', requestBody, { headers: headers })
      .map((resp: Response) => resp.json())
      .catch((error: any) => { return Observable.throw(error); });
  }
}
