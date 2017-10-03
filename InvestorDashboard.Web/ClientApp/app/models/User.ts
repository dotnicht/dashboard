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
    public userName: string;
    public fullName: string;
    public email: string;
    public jobTitle: string;
    public phoneNumber: string;
    public isEnabled: boolean;
    public isLockedOut: boolean;
    public roles: string[];
    // Note: Using only optional constructor properties without backing store disables typescript's type checking for the type
    constructor(id?: string, userName?: string, fullName?: string, email?: string, jobTitle?: string, phoneNumber?: string, roles?: string[]) {

        this.id = id;
        this.userName = userName;
        this.fullName = fullName;
        this.email = email;
        this.jobTitle = jobTitle;
        this.phoneNumber = phoneNumber;
        this.roles = roles;
    }


    get friendlyName(): string {
        let name = this.fullName || this.userName;

        if (this.jobTitle)
            name = this.jobTitle + ' ' + name;

        return name;
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

    constructor(email?: string, password?: string, confirmPassword?: string) {
        this.email = email;
        this.password = password;
        this.confirmPassword = confirmPassword;
    }
}


