/// <reference path="../../../../../../node_modules/@types/jasmine/index.d.ts" />
import { TestBed, async, ComponentFixture, ComponentFixtureAutoDetect } from '@angular/core/testing';
import { BrowserModule, By } from "@angular/platform-browser";
import { TfRecoveryCodesComponent } from './tf-recovery-codes.component';

let component: TfRecoveryCodesComponent;
let fixture: ComponentFixture<TfRecoveryCodesComponent>;

describe('TfRecoveryCodes component', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [ TfRecoveryCodesComponent ],
            imports: [ BrowserModule ],
            providers: [
                { provide: ComponentFixtureAutoDetect, useValue: true }
            ]
        });
        fixture = TestBed.createComponent(TfRecoveryCodesComponent);
        component = fixture.componentInstance;
    }));

    it('should do something', async(() => {
        expect(true).toEqual(true);
    }));
});