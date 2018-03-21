/// <reference path="../../../../node_modules/@types/jasmine/index.d.ts" />
import { TestBed, async, ComponentFixture, ComponentFixtureAutoDetect } from '@angular/core/testing';
import { BrowserModule, By } from "@angular/platform-browser";
import { ReferralComponent } from './referral.component';

let component: ReferralComponent;
let fixture: ComponentFixture<ReferralComponent>;

describe('referral component', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [ ReferralComponent ],
            imports: [ BrowserModule ],
            providers: [
                { provide: ComponentFixtureAutoDetect, useValue: true }
            ]
        });
        fixture = TestBed.createComponent(ReferralComponent);
        component = fixture.componentInstance;
    }));

    it('should do something', async(() => {
        expect(true).toEqual(true);
    }));
});