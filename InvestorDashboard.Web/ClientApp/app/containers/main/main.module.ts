import { NgModule } from '@angular/core';
import { MainComponent } from './main.component';
import { AlertService } from '../../services/alert.service';
import { ConfigurationService } from '../../services/configuration.service';
import { AppTitleService } from '../../services/app-title.service';
import { NotificationService } from '../../services/notification.service';
import { AccountEndpoint } from '../../services/account-endpoint.service';
import { AppTranslationService, TranslateLanguageLoader } from '../../services/app-translation.service';
import { AccountService } from '../../services/account.service';
import { NotificationEndpoint } from '../../services/notification-endpoint.service';
import { LocalStoreManager } from '../../services/local-store-manager.service';
import { EndpointFactory } from '../../services/endpoint-factory.service';
import { UserManageModule } from '../user-manage/user_manage.module';
import { AppRoutingModule, routingComponents } from '../../app.routing';
import { RouterModule } from '@angular/router';
import { MaterialModule } from '../../app.material.module';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../services/auth.service';
import { TranslateModule, TranslateLoader } from '@ngx-translate/core';
import { Utilities } from '../../services/utilities';


@NgModule({
  declarations: [
    MainComponent
  ],
  imports: [
    CommonModule,
    MaterialModule,
  
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
    Utilities,
    AlertService,
    ConfigurationService,
    LocalStoreManager,
    AppTranslationService,
    EndpointFactory
  ],
  exports: [MainComponent]
})
export class MainModule {
}
