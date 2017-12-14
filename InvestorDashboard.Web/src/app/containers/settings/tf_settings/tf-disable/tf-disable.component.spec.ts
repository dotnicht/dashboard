/// <reference path="../../../../../../node_modules/@types/jasmine/index.d.ts" />
import { TestBed, async, ComponentFixture, ComponentFixtureAutoDetect } from '@angular/core/testing';
import { BrowserModule, By } from "@angular/platform-browser";
import { TfDisableComponent } from './tf-disable.component';

let component: TfDisableComponent;
let fixture: ComponentFixture<TfDisableComponent>;

describe('TfDisable component', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [ TfDisableComponent ],
            imports: [ BrowserModule ],
            providers: [
                { provide: ComponentFixtureAutoDetect, useValue: true }
            ]
        });
        fixture = TestBed.createComponent(TfDisableComponent);
        component = fixture.componentInstance;
    }));

    it('should do something', async(() => {
        expect(true).toEqual(true);
    }));
});