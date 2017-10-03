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


