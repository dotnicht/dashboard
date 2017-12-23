/// <reference path="../../../../../node_modules/@types/jasmine/index.d.ts" />
import { TestBed, async, ComponentFixture, ComponentFixtureAutoDetect } from '@angular/core/testing';
import { BrowserModule, By } from "@angular/platform-browser";
import { LoginWithRecoveryCodeComponent } from './login-with-recovery-code.component';

let component: LoginWithRecoveryCodeComponent;
let fixture: ComponentFixture<LoginWithRecoveryCodeComponent>;

describe('LoginWithRecoveryCode component', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [ LoginWithRecoveryCodeComponent ],
            imports: [ BrowserModule ],
            providers: [
                { provide: ComponentFixtureAutoDetect, useValue: true }
            ]
        });
        fixture = TestBed.createComponent(LoginWithRecoveryCodeComponent);
        component = fixture.componentInstance;
    }));

    it('should do something', async(() => {
        expect(true).toEqual(true);
    }));
});