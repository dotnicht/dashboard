/// <reference path="../../../../../../node_modules/@types/jasmine/index.d.ts" />
import { TestBed, async, ComponentFixture, ComponentFixtureAutoDetect } from '@angular/core/testing';
import { BrowserModule, By } from "@angular/platform-browser";
import { TfRecoveryKeyComponent } from './tf-recovery-key.component';

let component: TfRecoveryKeyComponent;
let fixture: ComponentFixture<TfRecoveryKeyComponent>;

describe('TfRecoveryKey component', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [ TfRecoveryKeyComponent ],
            imports: [ BrowserModule ],
            providers: [
                { provide: ComponentFixtureAutoDetect, useValue: true }
            ]
        });
        fixture = TestBed.createComponent(TfRecoveryKeyComponent);
        component = fixture.componentInstance;
    }));

    it('should do something', async(() => {
        expect(true).toEqual(true);
    }));
});