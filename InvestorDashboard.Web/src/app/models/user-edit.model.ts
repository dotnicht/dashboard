import { User } from './user.model';


export class UserEdit extends User {
    public currentPassword: string;
    public newPassword: string;
    public confirmPassword: string;

    constructor(currentPassword?: string, newPassword?: string, confirmPassword?: string) {
        super();

        this.currentPassword = currentPassword;
        this.newPassword = newPassword;
        this.confirmPassword = confirmPassword;
    }
}

export class ChangePassWord {
    public oldPassword: string;
    public password: string;
    public confirmPassword: string;
}
export class ForgotPassWord {
    public email: string;
}