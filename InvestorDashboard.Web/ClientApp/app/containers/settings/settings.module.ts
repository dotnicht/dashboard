import { SettingsComponent } from './settings.component';
import { NgModule } from '@angular/core';
import { TranslateModule, TranslateLoader } from '@ngx-translate/core';
import { RouterModule, Routes } from '@angular/router';
import { TranslateLanguageLoader, AppTranslationService } from '../../services/app-translation.service';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { MaterialModule } from '../../app.material.module';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../services/auth.service';
import { AlertService } from '../../services/alert.service';
import { ConfigurationService } from '../../services/configuration.service';
import { LocalStoreManager } from '../../services/local-store-manager.service';
import { EndpointFactory } from '../../services/endpoint-factory.service';
import { UserInfoComponent } from '../../components/controls/user-info.component';
import { AccountService } from '../../services/account.service';
import { AccountEndpoint } from '../../services/account-endpoint.service';
import { CaseFormatterDirective } from '../../directives/case-formater.directive';


@NgModule({
    declarations: [
        SettingsComponent,
        UserInfoComponent,
        CaseFormatterDirective
    ],
    imports: [
        CommonModule,
        MaterialModule,
        FormsModule,
        ReactiveFormsModule,
        RouterModule,
        TranslateModule.forRoot({
            loader: {
                provide: TranslateLoader,
                useClass: TranslateLanguageLoader
            }
        })
    ],
    providers: [
        AccountService,
        AccountEndpoint,
        AuthService,
        AlertService,
        ConfigurationService,
        LocalStoreManager,
        AppTranslationService,
        EndpointFactory
    ],
    exports: [
        SettingsComponent
    ]
})
export class SettingsModule {
}