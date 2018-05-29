import { NgModule } from '@angular/core';
import { DashboardComponent, AddedQuestionDialogComponent, QrDialogComponent, UsdDialogComponent, EurDialogComponent } from './dashboard.component';
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

@NgModule({
    declarations: [
        DashboardComponent,
        AddedQuestionDialogComponent,
        TransferComponent,
        SuccsessTransferDialogComponent,
        FailedTransferDialogComponent,
        QrDialogComponent,
        EurDialogComponent,
        UsdDialogComponent
    ],
    imports: [
        CommonModule,
        ClipboardModule,
        MaterialModule,
        RouterModule,
        ReCaptchaModule,
        FormsModule,

        TranslateModule.forRoot({
            loader: {
                provide: TranslateLoader,
                useClass: TranslateLanguageLoader
            }
        })
    ],
    providers: [
        AppTranslationService,
        ClientInfoEndpointService,
        DashboardEndpoint
    ],
    entryComponents: [
        AddedQuestionDialogComponent,
        SuccsessTransferDialogComponent,
        FailedTransferDialogComponent,
        QrDialogComponent,
        EurDialogComponent,
        UsdDialogComponent
    ]
})
export class DashboardModule { }