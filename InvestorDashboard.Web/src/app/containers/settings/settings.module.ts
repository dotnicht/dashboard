import { SettingsComponent } from './settings.component';
import { NgModule } from '@angular/core';
import { TranslateModule, TranslateLoader } from '@ngx-translate/core';
import { RouterModule, Routes, RouterLinkActive } from '@angular/router';
import { TranslateLanguageLoader, AppTranslationService } from '../../services/app-translation.service';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { MaterialModule } from '../../app.material.module';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../services/auth.service';
import { ConfigurationService } from '../../services/configuration.service';
import { LocalStoreManager } from '../../services/local-store-manager.service';
import { UserInfoComponent } from '../../components/controls/user-info.component';
import { AccountService } from '../../services/account.service';
import { AccountEndpoint } from '../../services/account-endpoint.service';
import { CaseFormatterDirective } from '../../directives/case-formater.directive';
import { TfaComponent } from '../../components/controls/tfa/tfa.component';
import { RestorePasswordComponent } from '../../components/controls/restore-password/restore.password.component';
export const SETTINGS_ROUTES: Routes = [
    { path: '', redirectTo: 'profile', pathMatch: 'full' },
    { path: 'profile', component: UserInfoComponent },
    { path: '2fa', component: TfaComponent },
    { path: 'restore_password', component: RestorePasswordComponent }
];
@NgModule({
    declarations: [
        SettingsComponent,
        UserInfoComponent,
        TfaComponent,
        CaseFormatterDirective,
        RestorePasswordComponent
    ],
    imports: [
        CommonModule,
        MaterialModule,
        FormsModule,
        ReactiveFormsModule,
        RouterModule.forChild(SETTINGS_ROUTES),

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
        ConfigurationService,
        LocalStoreManager,
        AppTranslationService
    ],
    exports: [
        SettingsComponent
    ]
})
export class SettingsModule {
}
