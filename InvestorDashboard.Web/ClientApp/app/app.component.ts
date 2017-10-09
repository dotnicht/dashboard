﻿import { Component, OnInit, OnDestroy, Inject, ViewEncapsulation, RendererFactory2, PLATFORM_ID, ViewChildren, QueryList } from '@angular/core';
import { Router, NavigationEnd, ActivatedRoute, PRIMARY_OUTLET, NavigationStart } from '@angular/router';
import { Meta, Title, DOCUMENT, MetaDefinition } from '@angular/platform-browser';
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



@Component({
    selector: 'app',
    templateUrl: './app.component.html',
    styleUrls: ['./app.component.scss'],
    encapsulation: ViewEncapsulation.None
})
export class AppComponent implements OnInit, OnDestroy {
    isAppLoaded: boolean;
    isUserLoggedIn: boolean;
    shouldShowLoginModal: boolean;
    removePrebootScreen: boolean;

    appTitle = 'Investor Dashboard';
    // appLogo = require('../assets/images/logo.png');

    stickyToasties: number[] = [];

    @ViewChildren('loginModal,loginControl')
    modalLoginControls: QueryList<any>;

    loginModal: ModalDirective;
    loginControl: LoginComponent;


    // This will go at the END of your title for example "Home - Angular Universal..." <-- after the dash (-)
    private endPageTitle: string = 'Angular Universal and ASP.NET Core Starter';
    // If no Title is provided, we'll use a default one before the dash(-)
    private defaultPageTitle: string = 'My App';

    private routerSub$: Subscription;

    constructor(
        private toastyService: ToastyService,
        private toastyConfig: ToastyConfig,
        private alertService: AlertService,

        storageManager: LocalStoreManager,
        private authService: AuthService,
        private configurations: ConfigurationService,
        private translationService: AppTranslationService,
        private appTitleService: AppTitleService,
        private notificationService: NotificationService,
        private router: Router,
        private activatedRoute: ActivatedRoute,
        private title: Title,
        private meta: Meta,
        private linkService: LinkService,
        @Inject(REQUEST) private request
    ) {
        console.log(`What's our REQUEST Object look like?`);
        console.log(`The Request object only really exists on the Server, but on the Browser we can at least see Cookies`);
        console.log(this.request);

        storageManager.initialiseStorageSyncListener();

        translationService.addLanguages(['en']);
        translationService.setDefaultLanguage('en');

        this.toastyConfig.theme = 'bootstrap';
        this.toastyConfig.position = 'top-right';
        this.toastyConfig.limit = 100;
        this.toastyConfig.showClose = true;

        this.appTitleService.appName = this.appTitle;
    }

    ngOnInit() {
        // Change "Title" on every navigationEnd event
        // Titles come from the data.title property on all Routes (see app.routes.ts)
        this._changeTitleOnNavigation();

        this.isUserLoggedIn = this.authService.isLoggedIn;

        setTimeout(() => {
            if (this.isUserLoggedIn) {
                this.alertService.resetStickyMessage();

                // if (!this.authService.isSessionExpired)
                    this.alertService.showMessage('Login', `Welcome back ${this.userName}!`, MessageSeverity.default);
                // else
                //     this.alertService.showStickyMessage('Session Expired', 'Your Session has expired. Please log in again', MessageSeverity.warn);
            }
        }, 2000);
        if (isPlatformBrowser) {
            this.alertService.getDialogEvent().subscribe(alert => this.showDialog(alert));
        }
        this.alertService.getMessageEvent().subscribe(message => this.showToast(message, false));
        this.alertService.getStickyMessageEvent().subscribe(message => this.showToast(message, true));

        this.authService.reLoginDelegate = () => this.shouldShowLoginModal = true;

        this.authService.getLoginStatusEvent().subscribe(isLoggedIn => {
            this.isUserLoggedIn = isLoggedIn;

   

            setTimeout(() => {
                if (!this.isUserLoggedIn) {
                    this.alertService.showMessage('Session Ended!', '', MessageSeverity.default);
                }
            }, 500);
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
    showDialog(dialog: AlertDialog) {
        if (isPlatformBrowser) {
            var alertify: any = require('../app/assets/scripts/alertify.js');
            alertify.set({
                labels: {
                    ok: dialog.okLabel || 'OK',
                    cancel: dialog.cancelLabel || 'Cancel'
                }
            });

            switch (dialog.type) {
                case DialogType.alert:
                    alertify.alert(dialog.message);

                    break;
                case DialogType.confirm:
                    alertify
                        .confirm(dialog.message, (e) => {
                            if (e) {
                                dialog.okCallback();
                            }
                            else {
                                if (dialog.cancelCallback)
                                    dialog.cancelCallback();
                            }
                        });

                    break;
                case DialogType.prompt:
                    alertify
                        .prompt(dialog.message, (e, val) => {
                            if (e) {
                                dialog.okCallback(val);
                            }
                            else {
                                if (dialog.cancelCallback)
                                    dialog.cancelCallback();
                            }
                        }, dialog.defaultValue);

                    break;
                default: break;
            }
        }
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

