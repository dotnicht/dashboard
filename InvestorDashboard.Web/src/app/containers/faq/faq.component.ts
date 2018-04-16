import { Component, OnInit, Sanitizer } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../services/auth.service';
import { Http } from '@angular/http';
import { Observable } from 'rxjs/Observable';
import { environment } from '../../../environments/environment';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';

@Component({
    selector: 'app-faq',
    templateUrl: './faq.component.html',
    styleUrls: ['./faq.component.scss']
})
/** faq component*/
export class FaqComponent implements OnInit {
    /** faq ctor */
    pictures: any;
    constructor(private http: Http, private authService: AuthService, private sanitizer: DomSanitizer) { }

    /** Called by Angular after faq component initialized */
    ngOnInit(): void {
        this.getPictures().subscribe(data=> {
            console.log(data.json())
            this.pictures = data.json();
        });

    }

    getPictures(): Observable<Response> {
        let resp = this.http.get(environment.host + '/dashboard/pictures', this.authService.getAuthHeader())
            .map((response) => {
                return response;
            })
            .catch(err => {
                return Observable.throw(err);
            });

        return resp;
    }

    private getImage(base64img: string): SafeUrl {
        if (!base64img)
            return '';
        return this.sanitizer.bypassSecurityTrustUrl('data:image/* ;base64,' + base64img);
    }
}