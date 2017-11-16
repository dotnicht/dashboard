import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { NgModule } from '@angular/core';


import { AppComponent } from './app.component';
import { ResizeService } from './services/resize.service';
import { LocalStoreManager } from './services/local-store-manager.service';
import { DashboardModule } from './containers/dashboard/dashboard.module';
import { UserManageModule } from './containers/user-manage/user_manage.module';
import { MaterialModule } from './app.material.module';
import { CdkTableModule } from '@angular/cdk/table';
import { FormsModule } from '@angular/forms';
import { HttpModule } from '@angular/http';
import { CommonModule } from '@angular/common';
import { AppRoutingModule, routingComponents } from './app.routing';
import { FaqComponent } from './containers/faq/faq.component';
import { ClientInfoComponent } from './components/client_info/client_info.component';
import { ConfigurationService } from './services/configuration.service';
import { ClientInfoEndpointService } from './services/client-info.service';
import { AppTranslationService, TranslateLanguageLoader } from './services/app-translation.service';
import { TranslateService, TranslateModule, TranslateLoader } from '@ngx-translate/core';
import { TranslateStore } from '@ngx-translate/core/src/translate.store';
import { ClipboardModule } from 'ngx-clipboard';
import { SettingsModule } from './containers/settings/settings.module';
import { TransferHttpModule } from './modules/transfer-http/transfer-http.module';
import { AccountService } from './services/account.service';
import { AccountEndpoint } from './services/account-endpoint.service';


@NgModule({
  declarations: [
    AppComponent,
    ClientInfoComponent,
    FaqComponent,
    routingComponents
  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    CommonModule,
    ClipboardModule,
    HttpModule,
    FormsModule,

    CdkTableModule,
    AppRoutingModule,
    MaterialModule,
    UserManageModule,
    SettingsModule,
    DashboardModule,
    TranslateModule.forRoot({
      loader: {
        provide: TranslateLoader,
        useClass: TranslateLanguageLoader
      }
    }),
    TransferHttpModule
  ],
  providers: [
    AccountService,
    AccountEndpoint,
    ConfigurationService,
    ClientInfoEndpointService,
    AppTranslationService,
    TranslateStore,
    TranslateService,

    LocalStoreManager,
    ResizeService
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }