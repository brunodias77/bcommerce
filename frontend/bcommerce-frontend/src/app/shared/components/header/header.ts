import { CommonModule } from '@angular/common';
import { Component, HostListener, signal, OnDestroy, DestroyRef } from '@angular/core';
import { RouterModule, Router } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { fromEvent, throttleTime } from 'rxjs';
import { UserIcon } from '../../icons/user-icon/user-icon';
import { CartIcon } from '../../icons/cart-icon/cart-icon';
import { HeartIcon } from '../../icons/heart-icon/heart-icon';
import { AuthService } from '../../../services/auth/auth-service';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, RouterModule, UserIcon, CartIcon, HeartIcon],
  templateUrl: './header.html',
  styleUrl: './header.css',
})
export class Header implements OnDestroy {
  // Signals para controle de estado
  isScrolled = signal(false);
  isMobileMenuOpen = signal(false);
  cartItemsCount = signal(3);

  private readonly SCROLL_THRESHOLD = 50;
  private readonly SCROLL_THROTTLE_TIME = 16; // ~60fps

  constructor(
    private destroyRef: DestroyRef,
    private router: Router,
    private authService: AuthService
  ) {
    this.initScrollListener();
  }

  ngOnDestroy() {
    // Cleanup será feito automaticamente pelo takeUntilDestroyed
  }

  private initScrollListener() {
    // Versão otimizada com throttle para melhor performance
    fromEvent(window, 'scroll', { passive: true })
      .pipe(throttleTime(this.SCROLL_THROTTLE_TIME), takeUntilDestroyed(this.destroyRef))
      .subscribe(() => {
        this.updateScrollState();
      });
  }

  private updateScrollState() {
    const scrollTop = window.pageYOffset || document.documentElement.scrollTop;
    const shouldBeScrolled = scrollTop > this.SCROLL_THRESHOLD;

    // Só atualiza se o estado mudou (evita re-renders desnecessários)
    if (this.isScrolled() !== shouldBeScrolled) {
      this.isScrolled.set(shouldBeScrolled);
    }
  }

  // Fallback para o HostListener (caso o RxJS não funcione)
  @HostListener('window:scroll', [])
  onWindowScroll() {
    if (!this.isScrolled) {
      this.updateScrollState();
    }
  }

  scrollToTop() {
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  toggleMobileMenu() {
    this.isMobileMenuOpen.update((value) => !value);
  }

  closeMobileMenu() {
    this.isMobileMenuOpen.set(false);
  }

  headerHeight() {
    return this.isScrolled() ? 70 : 80;
  }

  // Classes computadas baseadas no estado do scroll
  headerClasses() {
    return this.isScrolled()
      ? 'left-4 right-4 top-4 rounded-2xl shadow-lg backdrop-blur-md bg-white/70'
      : 'left-0 right-0 top-0 rounded-none shadow-md bg-white';
  }

  containerClasses() {
    return this.isScrolled()
      ? 'max-w-7xl mx-auto px-6 py-3 transition-all duration-300'
      : 'max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-4 transition-all duration-300';
  }

  navClasses() {
    return 'transition-all duration-300 ease-in-out';
  }

  logoIconClasses() {
    return this.isScrolled()
      ? 'w-7 h-7 bg-blue-600 rounded-lg flex items-center justify-center transition-all duration-300 hover:scale-110 hover:bg-blue-700'
      : 'w-8 h-8 bg-blue-600 rounded-lg flex items-center justify-center transition-all duration-300 hover:scale-110 hover:bg-blue-700';
  }

  logoTextClasses() {
    return this.isScrolled()
      ? 'text-lg transition-all duration-300'
      : 'text-xl transition-all duration-300';
  }

  logoClasses() {
    return this.isScrolled()
      ? 'text-xl text-gray-900 transition-all duration-300 hover:text-blue-600'
      : 'text-2xl text-gray-900 transition-all duration-300 hover:text-blue-600';
  }

  searchBarClasses() {
    return this.isScrolled()
      ? 'hidden md:flex items-center flex-1 max-w-md mx-6 transition-all duration-300 opacity-100 transform translate-y-0'
      : 'hidden md:flex items-center flex-1 max-w-lg mx-8 transition-all duration-300 opacity-100 transform translate-y-0';
  }

  searchInputClasses() {
    return this.isScrolled()
      ? 'w-full px-4 py-2 pl-10 pr-4 text-sm border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all duration-200'
      : 'w-full px-4 py-3 pl-10 pr-4 text-base border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all duration-200';
  }

  linkClasses() {
    return this.isScrolled()
      ? 'text-sm text-gray-700 nav-link transition-colors duration-200'
      : 'text-base text-gray-700 nav-link transition-colors duration-200';
  }

  userActionsClasses() {
    return this.isScrolled()
      ? 'space-x-2 transition-all duration-300'
      : 'space-x-3 transition-all duration-300';
  }

  iconButtonClasses() {
    return this.isScrolled()
      ? 'p-1.5 hover:bg-gray-100 text-gray-600 hover:text-blue-600 transition-all duration-200'
      : 'p-2 hover:bg-gray-100 text-gray-600 hover:text-blue-600 transition-all duration-200';
  }

  iconClasses() {
    return this.isScrolled()
      ? 'w-5 h-5 transition-all duration-300'
      : 'w-6 h-6 transition-all duration-300';
  }

  cartBadgeClasses() {
    return this.isScrolled()
      ? 'absolute -top-1 -right-1 bg-yellow-primary text-black-primary text-xs rounded-full h-4 w-4 flex items-center justify-center animate-pulse font-medium transition-all duration-300'
      : 'absolute -top-1 -right-1 bg-yellow-primary text-black-primary text-xs rounded-full h-5 w-5 flex items-center justify-center animate-pulse font-medium transition-all duration-300';
  }

  mobileMenuClasses() {
    return 'border-gray-200 transition-all duration-300';
  }

  mobileLinkClasses() {
    return 'text-gray-700 transition-colors duration-200';
  }

  onUserButtonClick() {
    if (this.authService.isAuthenticated()) {
      this.router.navigate(['/profile']);
    } else {
      this.router.navigate(['/login']);
    }
  }
}
