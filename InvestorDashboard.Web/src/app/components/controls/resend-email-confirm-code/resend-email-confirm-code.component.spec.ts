/// <reference path="../../../../../node_modules/@types/jasmine/index.d.ts" />
import { TestBed, async, ComponentFixture, ComponentFixtureAutoDetect } from '@angular/core/testing';
import { BrowserModule, By } from "@angular/platform-browser";
import { ResendEmailConfirmCodeComponent } from './resend-email-confirm-code.component';

let component: ResendEmailConfirmCodeComponent;
let fixture: ComponentFixture<ResendEmailConfirmCodeComponent>;

describe('ResendEmailConfirmCode component', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [ ResendEmailConfirmCodeComponent ],
            imports: [ BrowserModule ],
            providers: [
                { provide: ComponentFixtureAutoDetect, useValue: true }
            ]
        });
        fixture = TestBed.createComponent(ResendEmailConfirmCodeComponent);
        component = fixture.componentInstance;
    }));

    it('should do something', async(() => {
        expect(true).toEqual(true);
    }));
});