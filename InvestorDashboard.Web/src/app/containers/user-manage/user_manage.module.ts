
import { NgModule } from '@angular/core';
import { LoginComponent } from '../../components/login/login.component';
import {
  RegisterComponent, RegisterRulesDialogComponent,
  ConfirmEmailDialogComponent, ConfirmedEmailComponent
} from '../../components/register/register.component';
import { RouterModule, PreloadAllModules } from '@angular/router';

import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { EqualValidator } from '../../directives/equal-validator.directive';
import { AuthService } from '../../services/auth.service';
import { ConfigurationService } from '../../services/configuration.service';
import { LocalStoreManager } from '../../services/local-store-manager.service';
import { AppTranslationService, TranslateLanguageLoader } from '../../services/app-translation.service';
import { TranslateService, TranslateModule, TranslateLoader } from '@ngx-translate/core';
import { MaterialModule } from '../../app.material.module';
import { ReCaptchaModule } from 'angular2-recaptcha';
import { ForgotPasswordComponent, ResetPasswordDialogComponent } from '../../components/controls/forgot-password/forgot.password.component';
import { ResetPasswordComponent } from '../../components/controls/reset-password/reset-password.component';
import { SharedModule } from '../../app.shared.module';


@NgModule({
  declarations: [
    LoginComponent,
    RegisterComponent,
    // EqualValidator,
    RegisterRulesDialogComponent,
    ConfirmEmailDialogComponent,
    ConfirmedEmailComponent,
    ForgotPasswordComponent,
    ResetPasswordDialogComponent,
    ResetPasswordComponent
  ],
  imports: [
    CommonModule,
    MaterialModule,
    FormsModule,
    ReCaptchaModule,
    ReactiveFormsModule,
    RouterModule,
    SharedModule,

    TranslateModule.forRoot({
      loader: {
        provide: TranslateLoader,
        useClass: TranslateLanguageLoader
      }
    })
  ],
  providers: [
    AuthService,
    ConfigurationService,
    LocalStoreManager,
    AppTranslationService
  ],
  entryComponents: [
    RegisterRulesDialogComponent,
    ConfirmEmailDialogComponent,
    ResetPasswordDialogComponent
  ],
  exports: [
    LoginComponent
  ]
})
export class UserManageModule {
}
