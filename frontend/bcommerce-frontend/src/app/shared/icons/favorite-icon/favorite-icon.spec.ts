import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FavoriteIcon } from './favorite-icon';

describe('FavoriteIcon', () => {
  let component: FavoriteIcon;
  let fixture: ComponentFixture<FavoriteIcon>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FavoriteIcon]
    })
    .compileComponents();

    fixture = TestBed.createComponent(FavoriteIcon);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
