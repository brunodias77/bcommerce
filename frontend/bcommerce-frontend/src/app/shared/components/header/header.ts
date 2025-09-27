import { CommonModule } from '@angular/common';
import { Component, HostListener, signal } from '@angular/core';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-header',
  imports: [CommonModule, RouterModule],
  templateUrl: './header.html',
  styleUrl: './header.css',
})
export class Header {
  // Signals para controle de estado
  isScrolled = signal(false);
  isMobileMenuOpen = signal(false);
  cartItemsCount = signal(3);

  scrollContent = [
    'Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.',
    'Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur.',
    'Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium.',
  ];

  @HostListener('window:scroll', [])
  onWindowScroll() {
    const scrollTop = window.pageYOffset || document.documentElement.scrollTop;
    this.isScrolled.set(scrollTop > 50);
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
      ? 'left-4 right-4 top-4 rounded-2xl shadow-lg'
      : 'left-0 right-0 top-0 rounded-none shadow-md';
  }

  containerClasses() {
    return this.isScrolled()
      ? 'max-w-7xl mx-auto px-6 py-3'
      : 'max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-4';
  }

  navClasses() {
    return this.isScrolled() ? 'transition-all duration-300' : 'transition-all duration-300';
  }

  logoIconClasses() {
    return this.isScrolled()
      ? 'w-7 h-7 bg-blue-600 rounded-lg flex items-center justify-center transition-all hover:scale-110'
      : 'w-8 h-8 bg-blue-600 rounded-lg flex items-center justify-center transition-all hover:scale-110';
  }

  logoTextClasses() {
    return this.isScrolled() ? 'text-lg' : 'text-xl';
  }

  logoClasses() {
    return this.isScrolled()
      ? 'text-xl text-gray-900 transition-all duration-300'
      : 'text-2xl text-gray-900 transition-all duration-300';
  }

  searchBarClasses() {
    return this.isScrolled()
      ? 'hidden md:flex items-center flex-1 max-w-md mx-6 transition-all duration-300 opacity-100'
      : 'hidden md:flex items-center flex-1 max-w-lg mx-8 transition-all duration-300 opacity-100';
  }

  searchInputClasses() {
    return this.isScrolled()
      ? 'w-full px-4 py-2 pl-10 pr-4 text-sm border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all'
      : 'w-full px-4 py-3 pl-10 pr-4 text-base border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all';
  }

  linkClasses() {
    return this.isScrolled() ? 'text-sm text-gray-700' : 'text-base text-gray-700';
  }

  userActionsClasses() {
    return this.isScrolled() ? 'space-x-2' : 'space-x-3';
  }

  iconButtonClasses() {
    return this.isScrolled()
      ? 'p-1.5 hover:bg-gray-100 text-gray-600 hover:text-blue-600'
      : 'p-2 hover:bg-gray-100 text-gray-600 hover:text-blue-600';
  }

  iconClasses() {
    return this.isScrolled() ? 'w-5 h-5' : 'w-6 h-6';
  }

  cartBadgeClasses() {
    return this.isScrolled()
      ? 'absolute -top-1 -right-1 bg-red-500 text-white text-xs rounded-full h-4 w-4 flex items-center justify-center animate-pulse font-medium'
      : 'absolute -top-1 -right-1 bg-red-500 text-white text-xs rounded-full h-5 w-5 flex items-center justify-center animate-pulse font-medium';
  }

  buttonClasses() {
    return 'hover:bg-gray-100';
  }

  mobileMenuClasses() {
    return 'border-gray-200';
  }

  mobileLinkClasses() {
    return 'text-gray-700';
  }
}
