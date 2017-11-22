/// <reference path="../../../../../node_modules/@types/jasmine/index.d.ts" />
import { TestBed, async, ComponentFixture, ComponentFixtureAutoDetect } from '@angular/core/testing';
import { BrowserModule, By } from "@angular/platform-browser";
import { ChangePasswordComponent } from './change-password.component';

let component: ChangePasswordComponent;
let fixture: ComponentFixture<ChangePasswordComponent>;

describe('change-password component', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [ ChangePasswordComponent ],
            imports: [ BrowserModule ],
            providers: [
                { provide: ComponentFixtureAutoDetect, useValue: true }
            ]
        });
        fixture = TestBed.createComponent(ChangePasswordComponent);
        component = fixture.componentInstance;
    }));

    it('should do something', async(() => {
        expect(true).toEqual(true);
    }));
});