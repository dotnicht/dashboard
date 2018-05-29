import { Component, OnInit, HostListener, AfterViewInit, OnDestroy } from '@angular/core';
import { DomSanitizer } from '@angular/platform-browser';
import { ActivatedRoute, Router, NavigationStart } from '@angular/router';
import { ResizeService } from './services/resize.service';
import { ClientInfoEndpointService } from './services/client-info.service';
import { AppTranslationService } from './services/app-translation.service';
import { ConfigurationService } from './services/configuration.service';
import { AuthService } from './services/auth.service';
import { LocalStoreManager } from './services/local-store-manager.service';
import { isPlatformBrowser, Location } from '@angular/common';
import { AccountService } from './services/account.service';
import { OtherService } from './services/other.service';
import { CookieService } from 'ngx-cookie-service';
import { ReferralService } from './services/referral.service';
import { Observable } from 'rxjs/Observable';
import { Utilities } from './services/utilities';
import { DashboardEndpoint } from './services/dashboard-endpoint.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit, OnDestroy {
  isMobile: boolean;
  isTab: boolean;


  appTitle = 'Data Trading';
  isAppLoaded: boolean;
  isUserLoggedIn: boolean;
  isReferralSystemDisabled = true;
  isAdmin = false;
  queryParams: any = null;

  observableList = [];

  public year: number;
  get showMainContainer() {
    return this.otherService.showMainComponent;
  }
  get selectedLanguage(): string {
    return this.translationService.getTranslation('languages.' + this.translationService.getCurrentLanguage);
  }

  constructor(
    private domSanitizer: DomSanitizer,
    private storageManager: LocalStoreManager,
    private dashboardService: DashboardEndpoint,
    private authService: AuthService,
    private accountService: AccountService,
    private configurations: ConfigurationService,
    private translationService: AppTranslationService,
    private otherService: OtherService,
    // private clientInfoService: ClientInfoEndpointService,
    private resizeService: ResizeService,
    private router: Router,
    private activatedRoute: ActivatedRoute,
    private cookieService: CookieService,
    // private clientInfoEndpointService: ClientInfoEndpointService,
    private location: Location,
    private referralService: ReferralService
  ) {

    storageManager.initialiseStorageSyncListener();

    translationService.addLanguages(['en', 'ru']);
    translationService.setDefaultLanguage('en');
    this.referralService.startUrl = window.location.href;

    this.activatedRoute.queryParams.subscribe(params => {
      this.referralService.queryParams = params;
      this.queryParams = params;
      if (Object.keys(params).length != 0) {
        this.location.replaceState(window.location.pathname, Utilities.serialize(params));
      }
    });
  }

  ngOnInit(): void {
    // if (window.location.pathname == "/presale") {
    //   window.location.href = 'http://www.racoin.io/';
    // }
    if (isPlatformBrowser) {
      this.year = new Date().getFullYear();
      this.resizeService.width = window.innerWidth;
      this.isMobile = this.resizeService.isMobile;
      this.isTab = this.resizeService.isTab;
    }
    this.otherService.showMainComponent = true;
    this.isUserLoggedIn = this.authService.isLoggedIn;

    this.observableList.push(this.dashboardService.dashboard$.subscribe(m => {
      this.isReferralSystemDisabled = m.icoInfoModel.isReferralSystemDisabled;
      this.isAdmin = m.clientInfoModel.isAdmin;
    }));

    // this.clientInfoEndpointService.icoInfo$.subscribe(data => {
    //   this.isReferralSystemDisabled = data.isReferralSystemDisabled;

    // });

    // this.clientInfoEndpointService.clientInfo$.subscribe(data => {
    //   this.isAdmin = data.isAdmin;
    // });

    this.authService.getLoginStatusEvent().subscribe(isLoggedIn => {
      this.isUserLoggedIn = isLoggedIn;
      if (isLoggedIn) {
        this.dashboardService.getDashboard().subscribe();
      }
    });

    // this.router.events.subscribe(event => {
    //   if (event instanceof NavigationStart) {
    //     let url = (<NavigationStart>event).url;

    //     if (url !== url.toLowerCase()) {
    //       this.router.navigateByUrl((<NavigationStart>event).url.toLowerCase());
    //     }
    //   }
    // });
  }


  ngOnDestroy(): void {
    this.observableList.map((el) => {
      el.unsubscribe();
    });
  }

  get userName(): string {
    return this.authService.currentUser ? this.authService.currentUser.userName : '';
  }

  @HostListener('window:resize', ['$event'])
  onResize(event) {
    if (isPlatformBrowser) {
      this.resizeService.width = window.innerWidth;
    }
    this.isMobile = this.resizeService.isMobile;
    this.isTab = this.resizeService.isTab;
    // this.isMobile = Utilities.isMobile();

  }

  selectLanguage(lang: string) {
    this.configurations.language = lang;

    //this.alertService.startLoadingMessage("", "Saving new defaults");

    this.accountService.updateUserPreferences(this.configurations.export())
      .subscribe(response => {
        // this.alertService.stopLoadingMessage();
        // this.alertService.showMessage("New Defaults", "Account defaults updated successfully", MessageSeverity.success)

      },
        error => {
          // this.alertService.stopLoadingMessage();
          // this.alertService.showStickyMessage("Save Error", `An error occured whilst saving configuration defaults.\r\nErrors: "${Utilities.getHttpResponseMessage(error)}"`,
          //   MessageSeverity.error, error);
        });

  }
}
