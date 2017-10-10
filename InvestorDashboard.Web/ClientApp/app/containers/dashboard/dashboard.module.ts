import { NgModule } from '@angular/core';
import { DashboardComponent } from './dashboard.component';
import { RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { MaterialModule } from '../../app.material.module';

@NgModule({
    declarations: [
        DashboardComponent
    ],
    imports: [
        MaterialModule,
        RouterModule
    ]
})
export class DashboardModule { }