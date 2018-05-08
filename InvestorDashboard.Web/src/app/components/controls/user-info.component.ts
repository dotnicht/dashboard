
import { Component, OnInit, ViewChild, Input, Inject } from '@angular/core';
import { MatDialogRef, MatDialog, MAT_DIALOG_DATA } from '@angular/material';
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
import { SafeUrl, DomSanitizer } from '@angular/platform-browser';
import { ClientInfoEndpointService } from '../../services/client-info.service';
import { DOCUMENT } from '@angular/platform-browser';
import { Router } from '@angular/router';



@Component({
    selector: 'user-info',
    templateUrl: './user-info.component.html',
    styleUrls: ['./user-info.component.scss']
})
export class UserInfoComponent implements OnInit {
    successKycMsgDialogRef: MatDialogRef<SuccessKycMsgDialogComponent> | null;
    failedKycMsgDialogRef: MatDialogRef<FailedKycMsgDialogComponent> | null;
    config = {
        data: {},
    };

    public mask: (string | RegExp)[];
    public phonePattern: string;

    public formResetToggle = true;

    public changesSavedCallback: () => void;
    public changesFailedCallback: () => void;
    public changesCancelledCallback: () => void;

    @Input()
    isViewOnly: boolean;
    isLoading = true;
    
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
    private errorClass: string;
    private imageCorrect: boolean;
    kycBonus: number;

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
        private papa: PapaParseService,
        private sanitizer: DomSanitizer,
        private clientInfoEndpointService: ClientInfoEndpointService,
        private dialog: MatDialog,
        @Inject(DOCUMENT) doc: any
    ) {
        dialog.afterOpen.subscribe(() => {
            if (!doc.body.classList.contains('no-scroll')) {
                doc.body.classList.add('no-scroll');
            }
        });
        dialog.afterAllClosed.subscribe(() => {
            doc.body.classList.remove('no-scroll');
        });
    }

    ngOnInit() {
        this.getCountryCode().subscribe(data => {
            let country = data as CountryCode[];

            this.countryCodes = country.filter((item) => {
                let equal = true;
                if (item.dial_code == null) {
                    return false;
                }
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

        this.clientInfoEndpointService.icoInfo$.subscribe(data => {
            this.kycBonus = data.kycBonus;
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

    private getImage(base64img: string): SafeUrl {
        if (!base64img)
            return '';
        return this.sanitizer.bypassSecurityTrustUrl('data:image/* ;base64,' + base64img);
    }

    private onCurrentUserDataLoadSuccessful(user: User) {
        //this.alertService.stopLoadingMessage();
        if (!user.firstName && !user.lastName && !user.countryCode && !user.city && !user.phoneCode && !user.phoneNumber && !user.photo && !user.telegramUsername) {
            this.user = new User();
            this.userEdit = new UserEdit();
            this.isEditMode = true;
        }
        else {
            this.user = user;
        }

        this.isLoading = false;
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

    loadPhoto(event: any) {
        let file: File = null;
        if (event.target.files.length > 0) {
            file = event.target.files[0];
            if (file.size <= 1024 * 1024) {
                let reader = new FileReader();
                reader.onload = this.handleReaderLoaded.bind(this);
                reader.readAsBinaryString(file);
                this.imageCorrect = true;
            }
            else {
                this.imageCorrect = false;
            }
        }

    }
    private isImageCorrect() {
        if ((this.imageCorrect == undefined && this.user.photo) || this.imageCorrect) {
            this.errorClass = '';
            this.imageCorrect = true;
        }
        else {
            this.errorClass = 'error';
            this.imageCorrect = false;
        }
    }
    private handleReaderLoaded(readerEvt) {
        var binaryString = readerEvt.target.result;
        this.userEdit.photo = btoa(binaryString);
    }

    private save() {
        if (this.isImageCorrect) {
            this.isSaving = true;
            this.userEdit.photo = this.userEdit.photo ? this.userEdit.photo : this.user.photo;
            //this.alertService.startLoadingMessage('Saving changes...');
            this.accountService.updateUser(this.userEdit).subscribe(response => this.saveSuccessHelper(undefined, response), error => this.saveFailedHelper(error));
        }
    }

    private trimWhitespaces() {

    }

    private afterSuccessSaving(user: User) {
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
    private saveSuccessHelper(user?: User, response?: any) {
        let kycStatuses = this.getKycStatuses(response.json());
        this.config.data = { kycStatuses: kycStatuses };

        if (kycStatuses.length > 0) {
            this.successKycMsgDialogRef = this.dialog.open(SuccessKycMsgDialogComponent, this.config);
            this.successKycMsgDialogRef.afterClosed().subscribe((result) => {
                if (result == true) {
                    this.afterSuccessSaving(user);

                }
            });
        }
        else {
            this.afterSuccessSaving(user);
        }
        this.config.data = { kycStatuses: [] };
    }

    private saveFailedHelper(error: any) {
        // this.failedKycMsgDialogRef = this.dialog.open(FailedKycMsgDialogComponent, this.config);
        // this.failedKycMsgDialogRef.afterClosed().subscribe((result) => {

        this.isSaving = false;
        if (this.changesFailedCallback)
            this.changesFailedCallback();
        // });


        // this.alertService.stopLoadingMessage();
        // this.alertService.showStickyMessage('Save Error', 'The below errors occured whilst saving your changes:', MessageSeverity.error, error);
        // this.alertService.showStickyMessage(error, null, MessageSeverity.error);

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
    private getKycStatusMessage(field: string, status: string, amount: number) {
        let kycStatuses = {
            Telegram: {
                true: `You were credited with ${amount} RACs for providing your telegram username.`,
                false: `You have removed you telegram username, ${amount} RACs were withdrawn from your airdrop bonus account. It doesn’t effect your main account.`
            },
            Profile: {
                true: `You were credited with ${amount} RACs for providing your personal data.`,
                false: `You have removed some your personal details, ${amount} RACs were withdrawn from your airdrop bonus account. It doesn’t effect your main account.`
            },
            Photo: {
                true: `You were credited with ${amount} RACs for providing your personal document. Please note, in case of providing false identity document the company has right to withdraw this airdrop tokens.`,
                false: `You have removed you personal document, ${amount} RACs were withdrawn from your airdrop bonus account. It doesn’t effect your main account.`
            },
            Legacy: {
                true: `You were credited with ${amount} RACs for providing your personal data.`,
                false: `You have removed some your personal details, ${amount} RACs were withdrawn from your airdrop bonus account. It doesn’t effect your main account.`
            }
        };

        if ((!('kycStatus' in this.user) && status == 'true') || (field == 'Legacy' && amount != null && 'kycStatus' in this.user) || 'kycStatus' in this.user) {
            this.updateUserKyc(field, status, amount);
            return kycStatuses[field][status];
        }
        return null;
    }

    private updateUserKyc(field: string, kycStatus: string, amount: number) {
        let status = kycStatus == 'true';

        if ('kycStatus' in this.user) {
            this.user.kycStatus[field].status = status;
            this.user.kycStatus[field].amount = amount;
        }
        else {
            this.user['kycStatus'] = {};
            this.user['kycStatus'][field] = {};
            this.user['kycStatus'][field][status.toString()] = status;
            this.user['kycStatus'][field]['amount'] = amount;
        }
    }

    private getKycStatuses(responseStatuses: any) {
        let messages: string[] = [];
        for (let field in responseStatuses) {
            if (!('kycStatus' in this.user) || responseStatuses[field].status != this.user.kycStatus[field].status) {
                let message = this.getKycStatusMessage(field, responseStatuses[field].status.toString(), responseStatuses[field].amount);
                if (message) { 
                    messages.push(message); 
                }
            }
        }
        return messages;
    }
}


@Component({
    selector: 'success-kyc-msg-dialog',
    templateUrl: './success-kyc-msg-dialog.component.html'
})
export class SuccessKycMsgDialogComponent {
    public kycStatuses: any;

    constructor(
        public dialogRef: MatDialogRef<SuccessKycMsgDialogComponent>,
        private router: Router,
        @Inject(MAT_DIALOG_DATA) public data: any) {
        this.kycStatuses = data.kycStatuses;
    }

    close() {
        this.dialogRef.close();
    }
}



@Component({
    selector: 'failed-kyc-msg-dialog',
    templateUrl: './failed-kyc-msg-dialog.component.html'
})
export class FailedKycMsgDialogComponent {
    public info: any;

    constructor(
        public dialogRef: MatDialogRef<SuccessKycMsgDialogComponent>,
        private router: Router,
        @Inject(MAT_DIALOG_DATA) public data: any) {
        this.info = data;
    }

    close() {
        this.dialogRef.close();
    }
}