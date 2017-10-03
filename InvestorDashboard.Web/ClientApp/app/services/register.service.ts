import { Injectable } from '@angular/core';
import { Http } from '@angular/http';
import { Response, Headers } from '@angular/http';
import { Observable } from 'rxjs/Observable';
import { UserRegister } from '../models/user.model';
import 'rxjs/add/operator/map';
import 'rxjs/add/operator/catch';
import 'rxjs/add/observable/throw';

@Injectable()
export class RegisterService {


    constructor(private http: Http) { }

    send(user: UserRegister) {
       
        let searchParams = new URLSearchParams();
        searchParams.append('Email', user.email);
        searchParams.append('Password', user.password);


        console.log(searchParams);
        let headers = new Headers();
        headers.append('Content-Type', 'application/json');

        return this.http.post('/account/register', JSON.stringify(user), { headers: headers })
            .map((resp: Response) => resp.json())
            .catch((error: any) => { return Observable.throw(error); });
    }


}