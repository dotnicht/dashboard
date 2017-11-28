/// <reference path="../../../../node_modules/@types/jasmine/index.d.ts" />
import { TestBed, async, ComponentFixture, ComponentFixtureAutoDetect } from '@angular/core/testing';
import { BrowserModule, By } from "@angular/platform-browser";
import { RegisterPreSaleComponent } from './register_presale.component';

let component: RegisterPreSaleComponent;
let fixture: ComponentFixture<RegisterPreSaleComponent>;

describe('register_presale component', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
          declarations: [RegisterPreSaleComponent ],
            imports: [ BrowserModule ],
            providers: [
                { provide: ComponentFixtureAutoDetect, useValue: true }
            ]
        });
        fixture = TestBed.createComponent(RegisterPreSaleComponent);
        component = fixture.componentInstance;
    }));

    it('should do something', async(() => {
        expect(true).toEqual(true);
    }));
});
