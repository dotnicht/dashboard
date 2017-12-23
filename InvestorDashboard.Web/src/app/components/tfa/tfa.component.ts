import { Component } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { Utilities } from '../../services/utilities';
import { Router } from '@angular/router';

class Code {
    value: string;
}

@Component({
    selector: 'app-tfa',
    templateUrl: './tfa.component.html',
    styleUrls: ['./tfa.component.scss']
})
export class TfaComponent {
    tfc: Code = new Code();
    errors = '';
    isLoading = false;
    constructor(private router: Router,
        private authService: AuthService) {

    }
    verify() {
        console.log(this.tfc);

        this.authService.loginWithTfa(this.tfc.value).subscribe(data => {
            this.router.navigate(['/']);
        },
            errors => {
                this.errors = Utilities.findHttpResponseMessage('error_description', errors);
            });
    }
}