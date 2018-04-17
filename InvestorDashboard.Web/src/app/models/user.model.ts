export class UserLogin {
    email: string;
    password: string;
    rememberMe: boolean;
    grant_type: string;
    scope: string;
    resource: string;

    constructor(email?: string, password?: string, rememberMe?: boolean) {
        this.email = email;
        this.password = password;
        this.rememberMe = rememberMe;
    }



}

export class User {
    public id: string;
    public address: string;
    public countryCode: string;
    public city: string;
    public userName: string;
    public firstName: string;
    public lastName: string;
    public email: string;
    public balance: number;
    public phoneCode: string;
    public phoneNumber: string;
    public twoFactorEnabled: boolean;
    public twoFactorValidated: boolean;
    public isEnabled: boolean;
    public isLockedOut: boolean;
    public photo: string;


    // Note: Using only optional constructor properties without backing store disables typescript's type checking for the type
    constructor(
        id?: string,
        userName?: string,
        email?: string,
        twofactorenabled?: string,
        firstName?: string,
        lastName?: string,
        phoneCode?: string,
        phoneNumber?: string,
        photo?: string,
        countryCode?: string,
    ) {
        this.id = id;
        this.userName = userName;
        if (twofactorenabled != undefined) {
            this.twoFactorEnabled = twofactorenabled.toLowerCase() == 'true';
        }
        this.email = email;
        this.firstName =firstName;
        this.lastName = lastName;
        this. phoneCode= phoneCode;
        this. phoneNumber = phoneNumber;
        this. photo = photo;
        this. countryCode = countryCode;
    }


}

export interface IUser {
    id: string;
    name: string;
}

export class UserRegister {
    email: string;
    password: string;
    confirmPassword: string;
    clickId: string;
    reCaptchaToken: String;
    referral: string;

    constructor(email?: string, password?: string, confirmPassword?: string) {
        this.email = email;
        this.password = password;
        this.confirmPassword = confirmPassword;
    }
}
export class RegisterRules {
    name: string;
    checked: boolean;
}


