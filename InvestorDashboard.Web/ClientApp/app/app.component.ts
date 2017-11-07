import { Component, OnInit, OnDestroy, Inject, ViewEncapsulation, RendererFactory2, PLATFORM_ID, ViewChildren, QueryList, HostListener, AfterViewInit } from '@angular/core';
import { Router, NavigationEnd, ActivatedRoute, PRIMARY_OUTLET, NavigationStart } from '@angular/router';
import { Meta, Title, DOCUMENT, MetaDefinition, DomSanitizer } from '@angular/platform-browser';
import { Subscription } from 'rxjs/Subscription';
import { isPlatformServer, isPlatformBrowser } from '@angular/common';
import { LinkService } from './shared/link.service';

import { REQUEST } from './shared/constants/request';
import { AuthService } from './services/auth.service';
import { AlertService, MessageSeverity, AlertMessage, DialogType, AlertDialog } from './services/alert.service';
import { ToastyService, ToastyConfig, ToastOptions, ToastData } from 'ng2-toasty';
import { ConfigurationService } from './services/configuration.service';
import { AppTranslationService } from './services/app-translation.service';
import { LocalStoreManager } from './services/local-store-manager.service';
import { NotificationService } from './services/notification.service';
import { AppTitleService } from './services/app-title.service';
import { ModalDirective } from 'ngx-bootstrap';
import { LoginComponent } from './components/login/login.component';
import { ResizeService } from './services/resize.service';
import { Utilities } from './services/utilities';
import { ClientInfoEndpointService } from './services/client-info.service';
import { Observable } from 'rxjs/Observable';



@Component({
    selector: 'app',
    templateUrl: './app.component.html',
    styleUrls: ['./app.component.scss', './green.theme.scss'],
    encapsulation: ViewEncapsulation.None
})
export class AppComponent implements OnInit, OnDestroy, AfterViewInit {
    loadTimer: number;

    isAppLoaded: boolean;
    isUserLoggedIn: boolean;
    shouldShowLoginModal: boolean;
    removePrebootScreen: boolean;


    appTitle = 'Data Trading';
    // appLogo = require('../app/assets/images/logo.png');

    stickyToasties: number[] = [];

    @ViewChildren('loginModal,loginControl')
    modalLoginControls: QueryList<any>;

    loginModal: ModalDirective;
    loginControl: LoginComponent;

    get selectedLanguage(): string {
        return this.translationService.getTranslation('languages.' + this.translationService.getCurrentLanguage);
    }

    public isMobile: boolean;
    public isTab: boolean;
    public year: number;
    // This will go at the END of your title for example "Home - Angular Universal..." <-- after the dash (-)
    private endPageTitle: string = 'Investor Dashboard';
    // If no Title is provided, we'll use a default one before the dash(-)
    private defaultPageTitle: string = 'Data Trading';

    private routerSub$: Subscription;
    private clientInfoSubscription: Subscription;

    @HostListener('window:resize', ['$event'])
    onResize(event) {
        if (isPlatformBrowser) {
            this.resizeService.width = window.innerWidth;
        }
        this.isMobile = this.resizeService.isMobile;
        this.isTab = this.resizeService.isTab;
        // this.isMobile = Utilities.isMobile();

    }

    constructor(
        private toastyService: ToastyService,
        private toastyConfig: ToastyConfig,
        private alertService: AlertService,
        private domSanitizer: DomSanitizer,
        private storageManager: LocalStoreManager,
        private authService: AuthService,
        private configurations: ConfigurationService,
        private translationService: AppTranslationService,
        private clientInfoService: ClientInfoEndpointService,
        private appTitleService: AppTitleService,
        private notificationService: NotificationService,
        private resizeService: ResizeService,
        private router: Router,
        private activatedRoute: ActivatedRoute,
        private title: Title,
        private meta: Meta,
        private linkService: LinkService,
        @Inject(REQUEST) private request
    ) {


        this.storageManager.initialiseStorageSyncListener();



        this.translationService.addLanguages(['en']);
        this.translationService.setDefaultLanguage('en');

        this.toastyConfig.theme = 'bootstrap';
        this.toastyConfig.position = 'top-right';
        this.toastyConfig.limit = 100;
        this.toastyConfig.showClose = true;

        this.appTitleService.appName = this.appTitle;

    }
    selectLanguage(lang: string) {
        this.configurations.language = lang;
    }
    ngOnInit() {


        let curDate = new Date();

        // Change "Title" on every navigationEnd event
        // Titles come from the data.title property on all Routes (see app.routes.ts)
        this._changeTitleOnNavigation();
        if (isPlatformBrowser) {
            this.year = new Date().getFullYear();
            this.resizeService.width = window.innerWidth;
            this.isMobile = this.resizeService.isMobile;
            this.isTab = this.resizeService.isTab;

        }

        this.isUserLoggedIn = this.authService.isLoggedIn;

        if (this.isUserLoggedIn) {
            this.refreshData();
        }

        this.alertService.getMessageEvent().subscribe(message => this.showToast(message, false));
        this.alertService.getStickyMessageEvent().subscribe(message => this.showToast(message, true));

        this.authService.getLoginStatusEvent().subscribe(isLoggedIn => {
            this.isUserLoggedIn = isLoggedIn;
        });

        this.router.events.subscribe(event => {
            if (event instanceof NavigationStart) {
                let url = (<NavigationStart>event).url;

                if (url !== url.toLowerCase()) {
                    this.router.navigateByUrl((<NavigationStart>event).url.toLowerCase());
                }
            }
        });


    }

    ngOnDestroy() {
        // Subscription clean-up
        this.routerSub$.unsubscribe();

        if (this.clientInfoSubscription) {
            this.clientInfoSubscription.unsubscribe();
        }

    }

    ngAfterViewInit() {

    }
    getYear() {
        return new Date().getUTCFullYear();
    }
    showToast(message: AlertMessage, isSticky: boolean) {

        if (message == null) {
            for (let id of this.stickyToasties.slice(0)) {
                this.toastyService.clear(id);
            }

            return;
        }

        let toastOptions: ToastOptions = {
            title: message.summary,
            msg: message.detail,
            timeout: isSticky ? 0 : 4000
        };


        if (isSticky) {
            toastOptions.onAdd = (toast: ToastData) => this.stickyToasties.push(toast.id);

            toastOptions.onRemove = (toast: ToastData) => {
                let index = this.stickyToasties.indexOf(toast.id, 0);

                if (index > -1) {
                    this.stickyToasties.splice(index, 1);
                }

                toast.onAdd = null;
                toast.onRemove = null;
            };
        }


        switch (message.severity) {
            case MessageSeverity.default: this.toastyService.default(toastOptions); break;
            case MessageSeverity.info: this.toastyService.info(toastOptions); break;
            case MessageSeverity.success: this.toastyService.success(toastOptions); break;
            case MessageSeverity.error: this.toastyService.error(toastOptions); break;
            case MessageSeverity.warn: this.toastyService.warning(toastOptions); break;
            case MessageSeverity.wait: this.toastyService.wait(toastOptions); break;
            default: break;
        }
    }

    public refreshData() {


        // this.subscribeToClientInfoData();
    }
    private subscribeToClientInfoData(): void {
        this.clientInfoSubscription = Observable.timer(30000).first().subscribe(() => this.refreshData());
    }
    private _changeTitleOnNavigation() {

        this.routerSub$ = this.router.events
            .filter(event => event instanceof NavigationEnd)
            .map(() => this.activatedRoute)
            .map(route => {
                while (route.firstChild) route = route.firstChild;
                return route;
            })
            .filter(route => route.outlet === 'primary')
            .mergeMap(route => route.data)
            .subscribe((event) => {
                this._setMetaAndLinks(event);
            });
    }

    private _setMetaAndLinks(event) {

        // Set Title if available, otherwise leave the default Title
        const title = event['title']
            ? `${event['title']} - ${this.endPageTitle}`
            : `${this.defaultPageTitle} - ${this.endPageTitle}`;

        this.title.setTitle(title);

        const metaData = event['meta'] || [];
        const linksData = event['links'] || [];

        for (let i = 0; i < metaData.length; i++) {
            this.meta.updateTag(metaData[i]);
        }

        for (let i = 0; i < linksData.length; i++) {
            this.linkService.addTag(linksData[i]);
        }
    }



    get userName(): string {
        return this.authService.currentUser ? this.authService.currentUser.userName : '';
    }
}
