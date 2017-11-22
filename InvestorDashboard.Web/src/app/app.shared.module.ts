import { NgModule } from "@angular/core";
import { EqualValidator } from "./directives/equal-validator.directive";

@NgModule({
    declarations: [EqualValidator],
    exports: [EqualValidator]
})

export class SharedModule { }