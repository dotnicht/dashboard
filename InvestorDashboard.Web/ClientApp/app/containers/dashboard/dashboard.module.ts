import { NgModule } from '@angular/core';
import { DashboardComponent } from './dashboard.component';
import { RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

@NgModule({
    declarations: [
        DashboardComponent
    ],
    imports: [
        RouterModule
    ]
})
export class DashboardModule { }