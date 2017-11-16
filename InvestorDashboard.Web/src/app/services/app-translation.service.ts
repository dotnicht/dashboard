
import { Injectable, Inject, PLATFORM_ID } from '@angular/core';
import { TranslateService, TranslateLoader } from '@ngx-translate/core';
import { Observable } from 'rxjs/Observable';
import { Subject } from 'rxjs/Subject';
import 'rxjs/add/observable/of';
import { isPlatformBrowser } from '@angular/common';


@Injectable()
export class AppTranslationService {

    private _languageChanged = new Subject<string>();
    readonly defaultLanguage = 'en';



    constructor( @Inject(PLATFORM_ID) private platformId: Object, private translate: TranslateService) {

        this.setDefaultLanguage(this.defaultLanguage);
    }


    addLanguages(lang: string[]) {
        this.translate.addLangs(lang);
    }


    setDefaultLanguage(lang: string) {
        this.translate.setDefaultLang(lang);
    }

    getDefaultLanguage() {
        return this.translate.defaultLang;
    }

    getBrowserLanguage() {
        return this.translate.getBrowserLang();
    }
    get getCurrentLanguage() {
        return this.translate.currentLang;
    }

    useBrowserLanguage(): string | void {
        return 'en';

        // let browserLang = this.getBrowserLanguage();

        // if (isPlatformBrowser(this.platformId)) {
        //     if (browserLang.match(/en|ru|fr|de|ar|ko/)) {
        //         this.changeLanguage(browserLang);
        //         return browserLang;
        //     }
        // } else {
        //     return 'en';
        // }
    }

    changeLanguage(language: string = 'en') {

        if (!language)
            language = this.translate.defaultLang;

        if (language != this.translate.currentLang) {
            setTimeout(() => {
                this.translate.use(language);
                this._languageChanged.next(language);
            });
        }

        return language;
    }


    getTranslation(key: string | Array<string>, interpolateParams?: Object): string | any {
        return this.translate.instant(key, interpolateParams);
    }


    getTranslationAsync(key: string | Array<string>, interpolateParams?: Object): Observable<string | any> {
        return this.translate.get(key, interpolateParams);
    }



    languageChangedEvent() {
        return this._languageChanged.asObservable();
    }

}




export class TranslateLanguageLoader implements TranslateLoader {

    /**
     * Gets the translations from webpack
     * @param lang
     * @returns {any}
     */
    public getTranslation(lang: string): any {

        //Note Dynamic require(variable) will not work. Require is always at compile time

        switch (lang) {
            case 'en':
                return Observable.of(require('../assets/locale/en.json'));
            // case 'ru':
            //     return Observable.of(require('../assets/locale/ru.json'));
            // case 'fr':
            //     return Observable.of(require('../assets/locale/fr.json'));
            // case 'de':
            //     return Observable.of(require('../assets/locale/de.json'));
            // case 'ar':
            //     return Observable.of(require('../assets/locale/ar.json'));
            // case 'ko':
            //     return Observable.of(require('../assets/locale/ko.json'));
            default:
        }
    }
}