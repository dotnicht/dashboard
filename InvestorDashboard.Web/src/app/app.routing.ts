import { NgModule } from '@angular/core';
import { Routes, RouterModule, PreloadAllModules } from '@angular/router';

import { NotFoundComponent } from './containers/not-found/not-found.component';
import { LoginComponent } from './components/login/login.component';
import { RegisterComponent, ConfirmedEmailComponent } from './components/register/register.component';
import { AuthService } from './services/auth.service';
import { AuthGuard } from './services/auth-guard.service';
import { DashboardComponent } from './containers/dashboard/dashboard.component';
import { SettingsComponent } from './containers/settings/settings.component';
import { UserInfoComponent } from './components/controls/user-info.component';
import { FaqComponent } from './containers/faq/faq.component';
import { ForgotPasswordComponent } from './components/controls/forgot-password/forgot.password.component';
import { ResetPasswordComponent } from './components/controls/reset-password/reset-password.component';
import { RegisterPreSaleComponent } from './containers/register_presale/register_presale.component';
import { TfaComponent } from './components/tfa/tfa.component';
import { TransferComponent } from './components/transfer/transfer.component';
import { LoginWithRecoveryCodeComponent } from './components/tfa/login-with-recovery-code/login-with-recovery-code.component';
import { ResendEmailConfirmCodeComponent } from './components/controls/resend-email-confirm-code/resend-email-confirm-code.component';
import { ReferralComponent } from './containers/referral/referral.component';

export const routingComponents = [
    NotFoundComponent
];


const routes: Routes = [
    {
        path: 'home',
        redirectTo: '/',
        pathMatch: 'full'
    },

    { path: '', redirectTo: '/dashboard', pathMatch: 'full' },
    { path: 'dashboard', component: DashboardComponent, canActivate: [AuthGuard] },
    { path: 'transfer', component: TransferComponent, canActivate: [AuthGuard] },
    { path: 'login', component: LoginComponent },
    { path: 'register', component: RegisterComponent },
    { path: 'forgot_password', component: ForgotPasswordComponent },
    { path: 'email_confirmed', component: ConfirmedEmailComponent },
    { path: 'resend_email_confirm_code', component: ResendEmailConfirmCodeComponent },
    // { path: 'presale', component: RegisterPreSaleComponent },
    { path: 'reset_password', component: ResetPasswordComponent, },
    { path: 'tfa', component: TfaComponent, },
    { path: 'login_with_recovery_code', component: LoginWithRecoveryCodeComponent, },
    {
        path: 'settings', canActivate: [AuthGuard],
        loadChildren: 'app/containers/settings/settings.module#SettingsModule'
    },
    // { path: 'referral', component: ReferralComponent, canActivate: [AuthGuard]},
    {
        path: 'faq', component: FaqComponent,
        data: {
            title: 'FAQ'
        }
    },
    { path: '**', component: NotFoundComponent }
];

@NgModule({
    imports: [
        RouterModule.forRoot(routes
            //    , { enableTracing: true }
        )],
    exports: [
        RouterModule
    ],
    providers: [
        AuthService, AuthGuard
    ]
})
export class AppRoutingModule { }

