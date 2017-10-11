import { NgModule } from '@angular/core';
import { DashboardComponent } from './dashboard.component';
import { RouterModule } from '@angular/router';
import { TranslateModule, TranslateLoader } from '@ngx-translate/core';
import { MaterialModule } from '../../app.material.module';
import { AppTranslationService, TranslateLanguageLoader } from '../../services/app-translation.service';
import { ClientInfoService } from '../../services/client-info.service';
import { DashboardService } from '../../services/dashboard.service';
import { CommonModule } from '@angular/common';

@NgModule({
    declarations: [
        DashboardComponent
    ],
    imports: [
        CommonModule,
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
        ClientInfoService,
        DashboardService
    ]
})
export class DashboardModule { }