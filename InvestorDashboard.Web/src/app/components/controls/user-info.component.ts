
import { Component, OnInit, ViewChild, Input } from '@angular/core';

import { AccountService } from '../../services/account.service';
import { Utilities } from '../../services/utilities';
import { User } from '../../models/user.model';
import { UserEdit } from '../../models/user-edit.model';
import { Role } from '../../models/role.model';
import { Permission } from '../../models/permission.model';
import { ConfigurationService } from '../../services/configuration.service';
import { CountryCode, Country } from '../../models/countryCodes';
import { Observable } from 'rxjs/Observable';
import { AppTranslationService } from '../../services/app-translation.service';
import { PapaParseService } from 'ngx-papaparse';


@Component({
    selector: 'user-info',
    templateUrl: './user-info.component.html',
    styleUrls: ['./user-info.component.scss']
})
export class UserInfoComponent implements OnInit {

    public mask: (string | RegExp)[];
    public phonePattern: string;

    public formResetToggle = true;

    public changesSavedCallback: () => void;
    public changesFailedCallback: () => void;
    public changesCancelledCallback: () => void;

    @Input()
    isViewOnly: boolean;

    private countryCodes: CountryCode[];
    private countries: Country[] = [];
    private isEditMode = false;
    private isNewUser = false;
    private isSaving = false;
    private isChangePassword = false;
    private isEditingSelf = false;
    private showValidationErrors = false;
    private editingUserName: string;
    private uniqueId: string = Utilities.uniqueId();
    private user: User = new User();
    private userEdit: UserEdit;
    get selectedCountry() {
        if (this.countries.length > 0 && this.user.countryCode != undefined) {
            return this.countries.find(x => x.key == this.user.countryCode).value;
        }
        return undefined;
    }


    @ViewChild('f')
    private form;

    // ViewChilds Required because ngIf hides template variables from global scope
    @ViewChild('userName')
    private userName;

    @ViewChild('userPassword')
    private userPassword;

    @ViewChild('email')
    private email;



    @ViewChild('currentPassword')
    private currentPassword;

    @ViewChild('newPassword')
    private newPassword;

    @ViewChild('confirmPassword')
    private confirmPassword;


    constructor(private accountService: AccountService,
        private configurationService: ConfigurationService,
        private translationService: AppTranslationService,
        private papa: PapaParseService) {

    }

    ngOnInit() {
        this.getCountryCode().subscribe(data => {
            let country = data as CountryCode[];

            this.countryCodes = country.filter((item) => {
                let equal = true;
                if (item.dial_code == null) {
                    return false;
                }

                // for (let element of country) {
                //     if (element.dial_code == item.dial_code) {
                //         equal = false;
                //     }
                // }
                // if (!equal) {
                //     return false;
                // }
                return true;
            }).sort((a, b) => {
                if (a.dial_code > b.dial_code) {
                    return 1;
                }
                if (a.dial_code < b.dial_code) {
                    return -1;
                }
                return 0;
            });

        });

        this.mask = ['+', '(', /\d/, /\d/, /\d/, ')', ' ', /\d/, /\d/, /\d/, '-', /\d/, /\d/, /\d/, /\d/];
        this.phonePattern = '\+?([0-9]{0,3})\(?([0-9]{3})\)?([ .-]?)([0-9]{3})-([0-9]{4})';
        this.loadCurrentUserData();

        this.getCountryList(this.translationService.getCurrentLanguage).subscribe(data => {
            for (let key in data) {
                let country = new Country();
                country.key = key;
                country.value = data[key];
                this.countries.push(country);
            }
        });


    }
    getCountryCode() {
        return Observable.of(require('../../assets/json/countryCodes.json'));
    }
    getCountryList(lang: string = 'en') {
        return Observable.of(require(`../../assets/json/country-list/country_${lang}.json`));
    }
    resetForm(replace = false) {
        this.isChangePassword = false;

        if (!replace) {
            this.form.reset();
        }
        else {
            this.formResetToggle = false;

            setTimeout(() => {
                this.formResetToggle = true;
            });
        }
    }


    editUser(user: User) {
        this.isNewUser = false;

        this.editingUserName = user.userName;
        this.user = new User();
        this.userEdit = new UserEdit();

        Object.assign(this.user, user);
        Object.assign(this.userEdit, user);
        this.edit();

        return this.userEdit;

    }


    displayUser(user: User) {

        this.user = new User();
        Object.assign(this.user, user);
        this.deletePasswordFromUser(this.user);
        this.isEditMode = false;
    }

    public deletePasswordFromUser(user: UserEdit | User) {
        let userEdit = <UserEdit>user;

        delete userEdit.currentPassword;
        delete userEdit.newPassword;
        delete userEdit.confirmPassword;
    }

    private loadCurrentUserData() {
        // this.alertService.startLoadingMessage();
        this.accountService.getUser().
            subscribe(user => this.onCurrentUserDataLoadSuccessful(user), error => this.onCurrentUserDataLoadFailed(error));
    }


    private onCurrentUserDataLoadSuccessful(user: User) {
        //this.alertService.stopLoadingMessage();
        this.user = user;
    }

    private onCurrentUserDataLoadFailed(error: any) {
        // this.alertService.stopLoadingMessage();
        // this.alertService.showStickyMessage('Load Error', `Unable to retrieve user data from the server.\r\nErrors: "${Utilities.getHttpResponseMessage(error)}"`,
        //     MessageSeverity.error, error);

        this.user = new User();
    }



    private showErrorAlert(caption: string, message: string) {
        //this.alertService.showMessage(caption, message, MessageSeverity.error);
    }





    private edit() {
        // if (!this.isGeneralEditor) {
        this.isEditingSelf = true;
        this.userEdit = new UserEdit();
        Object.assign(this.userEdit, this.user);
        // }
        // else {
        //     if (!this.userEdit)
        //         this.userEdit = new UserEdit();

        //     this.isEditingSelf = this.accountService.currentUser ? this.userEdit.id == this.accountService.currentUser.id : false;
        // }

        this.isEditMode = true;
        this.showValidationErrors = true;
        this.isChangePassword = false;
    }


    private save() {
        this.isSaving = true;
        //this.alertService.startLoadingMessage('Saving changes...');

        this.accountService.updateUser(this.userEdit).subscribe(response => this.saveSuccessHelper(), error => this.saveFailedHelper(error));
    }


    private saveSuccessHelper(user?: User) {

        if (user)
            Object.assign(this.userEdit, user);

        this.isSaving = false;
        //this.alertService.stopLoadingMessage();
        this.isChangePassword = false;
        this.showValidationErrors = false;

        this.deletePasswordFromUser(this.userEdit);
        Object.assign(this.user, this.userEdit);
        this.userEdit = new UserEdit();
        this.resetForm();


        // if (this.isGeneralEditor) {
        //     if (this.isNewUser)
        //         this.alertService.showMessage('Success', `User \"${this.user.userName}\" was created successfully`, MessageSeverity.success);
        //     else if (!this.isEditingSelf)
        //         this.alertService.showMessage('Success', `Changes to user \"${this.user.userName}\" was saved successfully`, MessageSeverity.success);
        // }

        if (this.isEditingSelf) {
            //this.alertService.showMessage('Success', 'Changes to your User Profile was saved successfully', MessageSeverity.success);
            this.refreshLoggedInUser();
        }

        this.isEditMode = false;


        if (this.changesSavedCallback)
            this.changesSavedCallback();
    }


    private saveFailedHelper(error: any) {
        this.isSaving = false;
        // this.alertService.stopLoadingMessage();
        // this.alertService.showStickyMessage('Save Error', 'The below errors occured whilst saving your changes:', MessageSeverity.error, error);
        // this.alertService.showStickyMessage(error, null, MessageSeverity.error);

        if (this.changesFailedCallback)
            this.changesFailedCallback();
    }





    private cancel() {
        // if (this.isGeneralEditor)
        //     this.userEdit = this.user = new UserEdit();
        // else
        this.userEdit = new UserEdit();

        this.showValidationErrors = false;
        this.resetForm();

        // this.alertService.showMessage('Cancelled', 'Operation cancelled by user', MessageSeverity.default);
        // this.alertService.resetStickyMessage();

        // if (!this.isGeneralEditor)
        this.isEditMode = false;

        if (this.changesCancelledCallback) {
            this.changesCancelledCallback();
        }
    }



    private refreshLoggedInUser() {
        this.accountService.refreshLoggedInUser()
            .subscribe(user => {
                this.loadCurrentUserData();
            },
            error => {
                // this.alertService.resetStickyMessage();
                // this.alertService.showStickyMessage('Refresh failed', 'An error occured whilst refreshing logged in user information from the server', MessageSeverity.error, error);
            });
    }


    private changePassword() {
        this.isChangePassword = true;
    }

}
