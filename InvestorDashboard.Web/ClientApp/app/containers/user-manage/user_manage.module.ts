
import { NgModule } from '@angular/core';
import { LoginComponent } from '../../components/login/login.component';
import { RegisterComponent } from '../../components/register/register.component';
import { RouterModule, PreloadAllModules } from '@angular/router';

import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { EqualValidator } from '../../directives/equal-validator.directive';
import { AuthService } from '../../services/auth.service';
import { AlertService } from '../../services/alert.service';
import { ConfigurationService } from '../../services/configuration.service';
import { LocalStoreManager } from '../../services/local-store-manager.service';
import { AppTranslationService, TranslateLanguageLoader } from '../../services/app-translation.service';
import { EndpointFactory } from '../../services/endpoint-factory.service';
import { TranslateService, TranslateModule, TranslateLoader } from '@ngx-translate/core';
import { MaterialModule } from '../../app.material.module';


@NgModule({
  declarations: [
    LoginComponent,
    RegisterComponent,
    EqualValidator
    
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
    AuthService,
    AlertService,
    ConfigurationService,
    LocalStoreManager,
    AppTranslationService,
    EndpointFactory
  ],
  exports: [
    LoginComponent
  ]
})
export class UserManageModule {
}
