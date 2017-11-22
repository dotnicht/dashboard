/// <reference path="../../../../../node_modules/@types/jasmine/index.d.ts" />
import { TestBed, async, ComponentFixture, ComponentFixtureAutoDetect } from '@angular/core/testing';
import { BrowserModule, By } from "@angular/platform-browser";
import { ResetPasswordComponent } from './reset-password.component';

let component: ResetPasswordComponent;
let fixture: ComponentFixture<ResetPasswordComponent>;

describe('reset-password component', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [ ResetPasswordComponent ],
            imports: [ BrowserModule ],
            providers: [
                { provide: ComponentFixtureAutoDetect, useValue: true }
            ]
        });
        fixture = TestBed.createComponent(ResetPasswordComponent);
        component = fixture.componentInstance;
    }));

    it('should do something', async(() => {
        expect(true).toEqual(true);
    }));
});