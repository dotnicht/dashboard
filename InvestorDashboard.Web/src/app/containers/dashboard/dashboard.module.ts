import { NgModule } from '@angular/core';
import { DashboardComponent, AddedQuestionDialogComponent } from './dashboard.component';
import { RouterModule } from '@angular/router';
import { TranslateModule, TranslateLoader } from '@ngx-translate/core';
import { MaterialModule } from '../../app.material.module';
import { AppTranslationService, TranslateLanguageLoader } from '../../services/app-translation.service';
import { ClientInfoEndpointService } from '../../services/client-info.service';
import { CommonModule } from '@angular/common';
import { DashboardEndpoint } from '../../services/dashboard-endpoint.service';
import { ClipboardModule } from 'ngx-clipboard';
import { FormsModule } from '@angular/forms';

@NgModule({
    declarations: [
        DashboardComponent,
        AddedQuestionDialogComponent
    ],
    imports: [
        CommonModule,
        ClipboardModule,
        MaterialModule,
        RouterModule,
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
        AddedQuestionDialogComponent
    ]
})
export class DashboardModule { }