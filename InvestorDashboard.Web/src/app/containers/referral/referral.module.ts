import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { TranslateModule, TranslateLoader } from '@ngx-translate/core';
import { MaterialModule } from '../../app.material.module';
import { AppTranslationService, TranslateLanguageLoader } from '../../services/app-translation.service';
import { ClientInfoEndpointService } from '../../services/client-info.service';
import { CommonModule } from '@angular/common';
import { DashboardEndpoint } from '../../services/dashboard-endpoint.service';
import { ClipboardModule } from 'ngx-clipboard';
import { FormsModule } from '@angular/forms';
import { TransferComponent, SuccsessTransferDialogComponent, FailedTransferDialogComponent } from '../../components/transfer/transfer.component';
import { ReCaptchaModule } from 'angular2-recaptcha';
import { ReferralComponent } from './referral.component';
import { ReferralService } from '../../services/referral.service';
import { HttpModule } from '@angular/http';
import { HttpClientModule } from '@angular/common/http';

@NgModule({
    declarations: [
        ReferralComponent,
    ],
    imports: [
        CommonModule,
        ClipboardModule,
        MaterialModule,
        RouterModule,
        ReCaptchaModule,
        FormsModule,
        HttpClientModule,
        TranslateModule.forRoot({
            loader: {
                provide: TranslateLoader,
                useClass: TranslateLanguageLoader
            }
        })
    ],
    providers: [
        AppTranslationService,
        ReferralService
        // ClientInfoEndpointService,
        // DashboardEndpoint
    ],
    entryComponents: [
        // AddedQuestionDialogComponent,
        // SuccsessTransferDialogComponent,
        // FailedTransferDialogComponent
    ]
})
export class ReferralModule { }