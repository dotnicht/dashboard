/// <reference path="../../../../node_modules/@types/jasmine/index.d.ts" />
import { TestBed, async, ComponentFixture, ComponentFixtureAutoDetect } from '@angular/core/testing';
import { BrowserModule, By } from '@angular/platform-browser';
import { ClientInfoComponent } from './client_info.component';

let component: ClientInfoComponent;
let fixture: ComponentFixture<ClientInfoComponent>;

describe('client_info component', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [ClientInfoComponent],
            imports: [BrowserModule],
            providers: [
                { provide: ComponentFixtureAutoDetect, useValue: true }
            ]
        });
        fixture = TestBed.createComponent(ClientInfoComponent);
        component = fixture.componentInstance;
    }));

    it('should do something', async(() => {
        expect(true).toEqual(true);
    }));
});