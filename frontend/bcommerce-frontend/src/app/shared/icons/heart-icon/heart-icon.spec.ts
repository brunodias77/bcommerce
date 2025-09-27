import { ComponentFixture, TestBed } from '@angular/core/testing';

import { HeartIcon } from './heart-icon';

describe('HeartIcon', () => {
  let component: HeartIcon;
  let fixture: ComponentFixture<HeartIcon>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [HeartIcon]
    })
    .compileComponents();

    fixture = TestBed.createComponent(HeartIcon);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
