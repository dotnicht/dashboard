import { NgModule } from '@angular/core';
import { DashboardComponent } from './dashboard.component';
import { RouterModule } from '@angular/router';
import { TranslateModule, TranslateLoader } from '@ngx-translate/core';
import { MaterialModule } from '../../app.material.module';
import { AppTranslationService, TranslateLanguageLoader } from '../../services/app-translation.service';
import { ClientInfoEndpointService } from '../../services/client-info.service';
import { CommonModule } from '@angular/common';
import { DashboardEndpoint } from '../../services/dashboard-endpoint.service';
import { ClipboardModule } from 'ngx-clipboard';

@NgModule({
    declarations: [
      DashboardComponent
    ],
    imports: [
        CommonModule,
        ClipboardModule,
        MaterialModule,
        RouterModule,

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
    ]
})
export class DashboardModule { }