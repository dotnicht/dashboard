
import { NgModule } from '@angular/core';
import { LoginComponent, TestDialog } from '../../components/login/login.component';
import { RegisterComponent } from '../../components/register/register.component';
import { RouterModule, PreloadAllModules } from '@angular/router';
import {
  MdButtonModule,
  MdCardModule,
  MdCheckboxModule,
  MdInputModule
} from '@angular/material';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { EqualValidator } from '../../directives/equal-validator.directive';
import { RegisterService } from '../../services/register.service';
import { AuthService } from '../../services/auth.service';
import { AlertService } from '../../services/alert.service';
import { ConfigurationService } from '../../services/configuration.service';
import { LocalStoreManager } from '../../services/local-store-manager.service';
import { AppTranslationService, TranslateLanguageLoader } from '../../services/app-translation.service';
import { EndpointFactory } from '../../services/endpoint-factory.service';
import { TranslateService, TranslateModule, TranslateLoader } from '@ngx-translate/core';

@NgModule({
  exports: [
    MdButtonModule,
    MdCardModule,
    MdCheckboxModule,
    MdInputModule
  ]
})
export class Material { }

@NgModule({
  declarations: [
    LoginComponent,
    RegisterComponent,
    EqualValidator,
    TestDialog
  ],
  imports: [
    CommonModule,
    Material,
    FormsModule,
    ReactiveFormsModule,
    RouterModule,
    TranslateModule.forRoot({
      loader: {
          provide: TranslateLoader,
          useClass: TranslateLanguageLoader
      }
  })
    // ,
    // RouterModule.forRoot([
    //   {
    //     path: 'login', component: LoginComponent,

    //     data: {
    //       title: 'Homepage'
    //     }
    //   }
    // ],
    //   {
    //     // Router options
    //     useHash: false,
    //     preloadingStrategy: PreloadAllModules,
    //     initialNavigation: 'enabled'
    //   })
  ],
  providers: [
    RegisterService,
    AuthService,
    AlertService,
    ConfigurationService,
    LocalStoreManager,
    AppTranslationService,
    EndpointFactory
  ],
  entryComponents: [TestDialog]
})
export class UserManageModule {
}
