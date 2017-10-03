
import { NgModule } from '@angular/core';
import { LoginComponent } from '../../components/login/login.component';
import { RegisterComponent } from '../../components/register/register.component';
import { RouterModule, PreloadAllModules } from '@angular/router';
import {
  MdButtonModule,
  MdCardModule,
  MdCheckboxModule,
  MdInputModule
} from '@angular/material';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { EqualValidator } from '../../directives/equal-validator.directive';
import { RegisterService } from '../../services/register.service';
import { AuthService } from '../../services/auth.service';

@NgModule({
  exports: [
    MdButtonModule,
    MdCardModule,
    MdCheckboxModule,
    MdInputModule
  ]
})
export class Material { }

@NgModule({
  declarations: [
    LoginComponent,
    RegisterComponent,
    EqualValidator
  ],
  imports: [
    CommonModule,
    Material,
    FormsModule,
    ReactiveFormsModule
    // ,
    // RouterModule.forRoot([
    //   {
    //     path: 'login', component: LoginComponent,

    //     data: {
    //       title: 'Homepage'
    //     }
    //   }
    // ],
    //   {
    //     // Router options
    //     useHash: false,
    //     preloadingStrategy: PreloadAllModules,
    //     initialNavigation: 'enabled'
    //   })
  ],
  providers: [
    RegisterService,
    AuthService
  ]
})
export class UserManageModule {
}
