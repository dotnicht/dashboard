
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
import {
  ForgotPasswordComponent,
  ForgotPasswordDialogComponent
} from '../../components/controls/forgot-password/forgot.password.component';
import { ResetPasswordComponent, ResetPasswordDialogComponent } from '../../components/controls/reset-password/reset-password.component';
import { SharedModule } from '../../app.shared.module';
import { RegisterPreSaleComponent } from '../register_presale/register_presale.component';
import { TfaComponent } from '../../components/tfa/tfa.component';
import { LoginWithRecoveryCodeComponent } from '../../components/tfa/login-with-recovery-code/login-with-recovery-code.component';


@NgModule({
  declarations: [
    LoginComponent,
    RegisterComponent,
    RegisterRulesDialogComponent,
    ConfirmEmailDialogComponent,
    ConfirmedEmailComponent,
    ForgotPasswordComponent,
    ForgotPasswordDialogComponent,
    ResetPasswordComponent,
    ResetPasswordDialogComponent,
    RegisterPreSaleComponent,
    TfaComponent,
    LoginWithRecoveryCodeComponent
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
    ForgotPasswordDialogComponent,
    ResetPasswordDialogComponent
  ],
  exports: [
    LoginComponent
  ]
})
export class UserManageModule {
}
