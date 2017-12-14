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

import { EqualValidator } from '../../directives/equal-validator.directive';
import { ChangePasswordComponent, ChangePasswordDialogComponent } from '../../components/controls/change-password/change-password.component';
import { SharedModule } from '../../app.shared.module';
import { PapaParseModule } from 'ngx-papaparse';
import { TfSettingsModule } from './tf_settings/tf_settings.module';


export const SETTINGS_ROUTES: Routes = [
    { path: '', redirectTo: 'profile', pathMatch: 'full' },
    { path: 'profile', component: UserInfoComponent },
    { path: '2fa', loadChildren: 'app/containers/settings/tf_settings/tf_settings.module#TfSettingsModule' },
    { path: 'change_password', component: ChangePasswordComponent }
];
@NgModule({
    declarations: [
        SettingsComponent,
        UserInfoComponent,
        CaseFormatterDirective,
        ChangePasswordComponent,
        ChangePasswordDialogComponent
    ],
    imports: [
        CommonModule,
        MaterialModule,
        SharedModule,
        FormsModule,
        ReactiveFormsModule,
        PapaParseModule,
        RouterModule.forChild([
            {
                path: '', component: SettingsComponent,
                children: SETTINGS_ROUTES
            }
        ]),

        TranslateModule.forRoot({
            loader: {
                provide: TranslateLoader,
                useClass: TranslateLanguageLoader
            }
        })
    ],
    entryComponents: [
        ChangePasswordDialogComponent
    ],
    providers: [
        AccountService,
        AccountEndpoint,
        AuthService,
        ConfigurationService,
        LocalStoreManager,
        AppTranslationService
    ]
})
export class SettingsModule {
}
