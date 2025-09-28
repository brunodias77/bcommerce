import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { ActivatePageComponent } from './activate-page';
import { AuthService } from '../../../../services/auth/auth-service';
import { ActivateAccountResponse } from '../../../../models/responses';

describe('ActivatePageComponent', () => {
  let component: ActivatePageComponent;
  let fixture: ComponentFixture<ActivatePageComponent>;
  let mockAuthService: jasmine.SpyObj<AuthService>;
  let mockRouter: jasmine.SpyObj<Router>;
  let mockActivatedRoute: any;

  beforeEach(async () => {
    const authServiceSpy = jasmine.createSpyObj('AuthService', ['activate'], {
      isActivating: jasmine.createSpy().and.returnValue(false),
      activateSuccess: jasmine.createSpy().and.returnValue(false),
      activateError: jasmine.createSpy().and.returnValue(null),
      activateMessage: jasmine.createSpy().and.returnValue(null),
      hasActivateErrors: jasmine.createSpy().and.returnValue(false)
    });
    
    const routerSpy = jasmine.createSpyObj('Router', ['navigate']);
    
    mockActivatedRoute = {
      queryParams: of({ token: 'test-token' })
    };

    await TestBed.configureTestingModule({
      imports: [ActivatePageComponent],
      providers: [
        { provide: AuthService, useValue: authServiceSpy },
        { provide: Router, useValue: routerSpy },
        { provide: ActivatedRoute, useValue: mockActivatedRoute }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ActivatePageComponent);
    component = fixture.componentInstance;
    mockAuthService = TestBed.inject(AuthService) as jasmine.SpyObj<AuthService>;
    mockRouter = TestBed.inject(Router) as jasmine.SpyObj<Router>;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should call activate service when token is provided', () => {
    const mockResponse: ActivateAccountResponse = { message: 'Account activated successfully' };
    mockAuthService.activate.and.returnValue(of(mockResponse));

    fixture.detectChanges();

    expect(mockAuthService.activate).toHaveBeenCalledWith({ token: 'test-token' });
  });

  it('should redirect to login when no token is provided', () => {
    mockActivatedRoute.queryParams = of({});
    
    fixture.detectChanges();

    expect(mockRouter.navigate).toHaveBeenCalledWith(['/login'], {
      queryParams: { error: 'Token de ativação não fornecido' }
    });
  });

  it('should handle activation success', () => {
    const mockResponse: ActivateAccountResponse = { message: 'Account activated successfully' };
    mockAuthService.activate.and.returnValue(of(mockResponse));

    fixture.detectChanges();

    expect(mockAuthService.activate).toHaveBeenCalled();
  });

  it('should handle activation error', () => {
    mockAuthService.activate.and.returnValue(throwError(() => new Error('Activation failed')));

    fixture.detectChanges();

    expect(mockAuthService.activate).toHaveBeenCalled();
  });

  it('should navigate to login on return button click', () => {
    component.onReturnToLogin();

    expect(mockRouter.navigate).toHaveBeenCalledWith(['/login']);
  });

  it('should retry activation on try again button click', () => {
    const mockResponse: ActivateAccountResponse = { message: 'Account activated successfully' };
    mockAuthService.activate.and.returnValue(of(mockResponse));
    
    fixture.detectChanges();
    
    // Reset the spy to check if it's called again
    mockAuthService.activate.calls.reset();
    
    component.onTryAgain();

    expect(mockAuthService.activate).toHaveBeenCalledWith({ token: 'test-token' });
  });
});