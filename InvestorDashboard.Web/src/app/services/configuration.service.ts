﻿
import { Injectable } from '@angular/core';

import { AppTranslationService } from './app-translation.service';
import { LocalStoreManager } from './local-store-manager.service';
import { DBkeys } from './db-Keys';
import { Utilities } from './utilities';



type UserConfiguration = {
    language: string,
    homeUrl: string,
    theme: string
};

@Injectable()
export class ConfigurationService {

    public static readonly appVersion: string = "1.0.0";

    public baseUrl: string = Utilities.baseUrl().replace(/\/$/, '');
    public fallbackBaseUrl: string = "https://dashboard.racoin.io/";
    public loginUrl: string = "/login";

    //***Specify default configurations here***
    public static readonly defaultLanguage: string = "en";
    public static readonly defaultHomeUrl: string = "/";
    public static readonly defaultTheme: string = "Default";
    //***End of defaults***  

    private _language: string = null;
    private _homeUrl: string = null;
    private _theme: string = null;


    constructor(private localStorage: LocalStoreManager, private translationService: AppTranslationService) {
        this.loadLocalChanges();
    }



    public loadLocalChanges() {

        if (this.localStorage.exists(DBkeys.LANGUAGE)) {
            this._language = this.localStorage.getDataObject<string>(DBkeys.LANGUAGE);
            this.translationService.changeLanguage(this._language);
        }
        else {
            this.resetLanguage();
        }

        if (this.localStorage.exists(DBkeys.HOME_URL))
            this._homeUrl = this.localStorage.getDataObject<string>(DBkeys.HOME_URL);

        if (this.localStorage.exists(DBkeys.THEME))
            this._theme = this.localStorage.getDataObject<string>(DBkeys.THEME);
    }


    private saveToLocalStore(data: any, key: string) {
        setTimeout(() => this.localStorage.savePermanentData(data, key));
    }


    public import(jsonValue: string) {

        this.clearLocalChanges();

        if (!jsonValue)
            return;

        let importValue: UserConfiguration = Utilities.JSonTryParse(jsonValue);

        if (importValue.language != null)
            this.language = importValue.language;

        if (importValue.homeUrl != null)
            this.homeUrl = importValue.homeUrl;

        if (importValue.theme != null)
            this.theme = importValue.theme;
    }


    public export(changesOnly = true): string {

        let exportValue: UserConfiguration =
            {
                language: changesOnly ? this._language : this.language,
                homeUrl: changesOnly ? this._homeUrl : this.homeUrl,
                theme: changesOnly ? this._theme : this.theme
            };

        return JSON.stringify(exportValue);
    }


    public clearLocalChanges() {
        this._language = null;
        this._homeUrl = null;
        this._theme = null;

        this.localStorage.deleteData(DBkeys.LANGUAGE);
        this.localStorage.deleteData(DBkeys.HOME_URL);
        this.localStorage.deleteData(DBkeys.THEME);

        this.resetLanguage();
    }


    private resetLanguage() {
        let language = this.translationService.useBrowserLanguage();

        if (language) {
            this._language = language;
        }
        else {
            this._language = this.translationService.changeLanguage()
        }
    }


    set language(value: string) {
        this._language = value;
        this.saveToLocalStore(value, DBkeys.LANGUAGE);
        this.translationService.changeLanguage(value);
    }
    get language() {
        if (this._language != null)
            return this._language;

        return ConfigurationService.defaultLanguage;
    }


    set homeUrl(value: string) {
        this._homeUrl = value;
        this.saveToLocalStore(value, DBkeys.HOME_URL);
    }
    get homeUrl() {
        if (this._homeUrl != null)
            return this._homeUrl;

        return ConfigurationService.defaultHomeUrl;
    }


    set theme(value: string) {
        this._theme = value;
        this.saveToLocalStore(value, DBkeys.THEME);
    }
    get theme() {
        if (this._theme != null)
            return this._theme;

        return ConfigurationService.defaultTheme;
    }

}