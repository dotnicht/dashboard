import { NgModule } from '@angular/core';
import { Routes, RouterModule, PreloadAllModules } from '@angular/router';

import { HomeComponent } from './containers/home/home.component';
import { NotFoundComponent } from './containers/not-found/not-found.component';
import { LoginComponent } from './components/login/login.component';
import { RegisterComponent } from './components/register/register.component';
import { AuthService } from './services/auth.service';
import { AuthGuard } from './services/auth-guard.service';
import { DashboardComponent } from './containers/dashboard/dashboard.component';

export const routingComponents = [
    HomeComponent, NotFoundComponent
];

const routes: Routes = [
    {
        path: 'home',
        redirectTo: '/',
        pathMatch: 'full'
    },
    {
        path: '', component: DashboardComponent, canActivate: [AuthGuard],
        // *** SEO Magic ***
        // We're using "data" in our Routes to pass in our <title> <meta> <link> tag information
        // Note: This is only happening for ROOT level Routes, you'd have to add some additional logic if you wanted this for Child level routing
        // When you change Routes it will automatically append these to your document for you on the Server-side
        //  - check out app.component.ts to see how it's doing this
        data: {
            title: '',
            meta: [{ name: 'description', content: '' }],
            links: [
                { rel: 'canonical', href: '' },
                { rel: 'alternate', hreflang: 'es', href: '' }
            ]
        }
    },
    {
        path: 'login', component: LoginComponent,
        data: {
            title: 'Login'
        }
    },
    {
        path: 'register', component: RegisterComponent,
        data: {
            title: 'Register'
        }
    },
    {
        path: '**', component: NotFoundComponent,
        data: {
            title: '404 - Not found',
            meta: [{ name: 'description', content: '404 - Error' }],
            links: [
                { rel: 'canonical', href: '' },
                { rel: 'alternate', hreflang: 'es', href: '' }
            ]
        }
    }
];

@NgModule({
    imports: [RouterModule.forRoot(routes)],
    exports: [
        RouterModule
    ],
    providers: [
        AuthService, AuthGuard
    ]
})
export class AppRoutingModule { }

