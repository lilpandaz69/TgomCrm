import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Sale } from './sale';

describe('Sale', () => {
  let component: Sale;
  let fixture: ComponentFixture<Sale>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [Sale]
    })
    .compileComponents();

    fixture = TestBed.createComponent(Sale);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
