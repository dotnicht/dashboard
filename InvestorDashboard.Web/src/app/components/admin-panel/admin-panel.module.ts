import { AdminPanelComponent, ConfirmExtraTokensDialogComponent } from "./admin-panel.component";
import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { MaterialModule } from "../../app.material.module";
import { RouterModule } from "@angular/router";
import { FormsModule } from "@angular/forms";
import { HttpClientModule } from "@angular/common/http";
import { TranslateModule, TranslateLoader } from "@ngx-translate/core";
import { TranslateLanguageLoader, AppTranslationService } from "../../services/app-translation.service";
import { AdminPanelService } from "../../services/admin-panel.service";
import { AuthService } from "../../services/auth.service";
import { HttpModule } from "@angular/http";

@NgModule({
    declarations: [
        AdminPanelComponent,
        ConfirmExtraTokensDialogComponent
    ],
    imports: [
        CommonModule,
        MaterialModule,
        RouterModule,
        FormsModule,
        TranslateModule.forRoot({
            loader: {
                provide: TranslateLoader,
                useClass: TranslateLanguageLoader
            }
        })
    ],
    providers: [
        AppTranslationService,
        AdminPanelService,
        AuthService
    ],
    entryComponents: [
        ConfirmExtraTokensDialogComponent
    ]
})
export class AdminPanelModule { }