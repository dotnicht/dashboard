/// <reference path="../../../../../../node_modules/@types/jasmine/index.d.ts" />
import { TestBed, async, ComponentFixture, ComponentFixtureAutoDetect } from '@angular/core/testing';
import { BrowserModule, By } from "@angular/platform-browser";
import { TfEnableComponent } from './tf-enable.component';

let component: TfEnableComponent;
let fixture: ComponentFixture<TfEnableComponent>;

describe('TfEnable component', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [ TfEnableComponent ],
            imports: [ BrowserModule ],
            providers: [
                { provide: ComponentFixtureAutoDetect, useValue: true }
            ]
        });
        fixture = TestBed.createComponent(TfEnableComponent);
        component = fixture.componentInstance;
    }));

    it('should do something', async(() => {
        expect(true).toEqual(true);
    }));
});