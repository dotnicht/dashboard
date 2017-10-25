import { NgModule, Inject } from '@angular/core';
import { RouterModule, PreloadAllModules } from '@angular/router';
import { CommonModule, APP_BASE_HREF } from '@angular/common';
import { HttpModule, Http } from '@angular/http';
import { FormsModule } from '@angular/forms';

import { Ng2BootstrapModule, ModalModule } from 'ngx-bootstrap';
import { NgxDatatableModule } from '@swimlane/ngx-datatable';
// i18n support
import { TranslateModule, TranslateLoader, TranslateService } from '@ngx-translate/core';
import { TranslateHttpLoader } from '@ngx-translate/http-loader';

import { AppComponent } from './app.component';

import { CdkTableModule } from '@angular/cdk/table';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';


import { UserManageModule } from './containers/user-manage/user_manage.module';

import { LinkService } from './shared/link.service';
import { UserService } from './shared/user.service';
import { ConnectionResolver } from './shared/route.resolver';
import { ORIGIN_URL } from './shared/constants/baseurl.constants';
import { TransferHttpModule } from '../modules/transfer-http/transfer-http.module';
import { AppRoutingModule, routingComponents } from './app.routing';
import { AlertService } from './services/alert.service';
import { ToastyModule } from 'ng2-toasty';
import { NotificationService } from './services/notification.service';
import { NotificationEndpoint } from './services/notification-endpoint.service';

import { ConfigurationService } from './services/configuration.service';
import { LocalStoreManager } from './services/local-store-manager.service';
import { AppTranslationService, TranslateLanguageLoader } from './services/app-translation.service';
import { TranslateStore } from '@ngx-translate/core/src/translate.store';
import { MaterialModule } from './app.material.module';
import { AppTitleService } from './services/app-title.service';
import { LoginComponent } from './components/login/login.component';
import { ClientInfoComponent } from './components/client_info/client_info.component';

import { ResponsiveDirective } from './directives/responsive';
import { ResizeService } from './services/resize.service';
import { DashboardModule } from './containers/dashboard/dashboard.module';
import { ClientInfoEndpointService } from './services/client-info.service';
import { SettingsModule } from './containers/settings/settings.module';

@NgModule({
    declarations: [
        AppComponent,
        ClientInfoComponent,

        routingComponents,
        ResponsiveDirective
    ],
    imports: [
        CommonModule,
        HttpModule,
        FormsModule,

        CdkTableModule,
        AppRoutingModule,
        MaterialModule,
        UserManageModule,
        DashboardModule,
        SettingsModule,
        TranslateModule.forRoot({
            loader: {
                provide: TranslateLoader,
                useClass: TranslateLanguageLoader
            }
        }),

        Ng2BootstrapModule.forRoot(), // You could also split this up if you don't want the Entire Module imported
        ToastyModule.forRoot(),

        TransferHttpModule // Our Http TransferData method


    ],
    providers: [
        AlertService,
        AppTitleService,

        LinkService,
        ConfigurationService,
        ClientInfoEndpointService,
        AppTranslationService,
        TranslateStore,
        TranslateService,

        ConnectionResolver,
        LocalStoreManager,
        ResizeService,
        NotificationService,
        NotificationEndpoint
    ]
})
export class AppModuleShared {
}
