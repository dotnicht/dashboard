import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { EthAddressComponent } from './eth-address.component';

describe('EthAddressComponent', () => {
  let component: EthAddressComponent;
  let fixture: ComponentFixture<EthAddressComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ EthAddressComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(EthAddressComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
