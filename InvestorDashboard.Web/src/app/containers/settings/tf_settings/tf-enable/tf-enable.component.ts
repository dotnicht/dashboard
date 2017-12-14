import { Component, OnInit, ViewChild } from '@angular/core';
import { AccountEndpoint } from '../../../../services/account-endpoint.service';
import { Utilities } from '../../../../services/utilities';
import { TfSettingsComponent } from '../tf_settings.component';

declare var QRCode: any;

class EnableAuthenticatorModel {
    code: string;
    sharedKey: string;
    authenticatorUri: string;
}

@Component({
    selector: 'app-tf-enable',
    templateUrl: './tf-enable.component.html',
    styleUrls: ['./tf-enable.component.scss']
})
export class TfEnableComponent implements OnInit {

    ea: EnableAuthenticatorModel = new EnableAuthenticatorModel();
    isLoading = false;
    errors: string;
    constructor(private accountEndpoint: AccountEndpoint) {

    }


    ngOnInit(): void {
        console.log('emit')
        this.accountEndpoint.TfGetActivationDataEndpoint().subscribe(data => {
            this.ea = data.json() as EnableAuthenticatorModel;
            this.qrInitialize(this.ea.authenticatorUri);
        });
    }
    qrInitialize(data: string) {
        document.getElementById('qrCode').innerHTML = '';
        let qrCode = new QRCode(document.getElementById('qrCode'), {
            text: data,
            width: 200,
            height: 200
        });
        document.getElementById('qrCode').getElementsByTagName('img')[0].style.display = 'none';
        document.getElementById('qrCode').getElementsByTagName('canvas')[0].style.display = 'block';
    }
    verify() {
        this.accountEndpoint.TfPostActivationDataEndpoint(this.ea.code).subscribe(data => {
            this.ea = data.json() as EnableAuthenticatorModel;
            this.qrInitialize(this.ea.authenticatorUri);
        },
            error => {
                this.errors = Utilities.findHttpResponseMessage('error_description', error);
            });
    }

}