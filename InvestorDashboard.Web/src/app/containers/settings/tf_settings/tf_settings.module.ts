import { NgModule } from "@angular/core";
import { TfSettingsComponent } from "./tf_settings.component";
import { CommonModule } from "@angular/common";
import { MaterialModule } from "../../../app.material.module";
import { AppTranslationService, TranslateLanguageLoader } from "../../../services/app-translation.service";
import { TranslateLoader, TranslateModule } from "@ngx-translate/core";
import { FormsModule } from "@angular/forms";
import { ReCaptchaModule } from "angular2-recaptcha";
import { RouterModule, Routes } from "@angular/router";
import { TfEnableComponent } from "./tf-enable/tf-enable.component";
import { TfDisableComponent } from "./tf-disable/tf-disable.component";
import { BrowserAnimationsModule } from "@angular/platform-browser/animations";
import { BrowserModule } from "@angular/platform-browser";
import { TfRecoveryCodesComponent } from "./tf-recovery-codes/tf-recovery-codes.component";
import { TfRecoveryKeyComponent } from "./tf-recovery-key/tf-recovery-key.component";


const TF_SETTINGS_ROUTES: Routes = [
    { path: 'enable', component: TfEnableComponent }
];

@NgModule({
    declarations: [
        TfSettingsComponent,
        TfEnableComponent,
        TfDisableComponent,
        TfRecoveryCodesComponent,
        TfRecoveryKeyComponent
    ],
    imports: [
        CommonModule,
        MaterialModule,
        RouterModule,
        ReCaptchaModule,
        FormsModule,
        RouterModule.forChild([
            {
                path: '', component: TfSettingsComponent,
                children: TF_SETTINGS_ROUTES
            }
        ]),
        TranslateModule.forRoot({
            loader: {
                provide: TranslateLoader,
                useClass: TranslateLanguageLoader
            }
        })
    ],
    providers: [
        AppTranslationService
    ]
})
export class TfSettingsModule { }