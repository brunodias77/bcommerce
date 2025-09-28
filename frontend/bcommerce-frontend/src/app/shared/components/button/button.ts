import { Component, computed, Input, input, output } from '@angular/core';
export type ButtonVariant = 'primary' | 'secondary' | 'danger' | 'success' | 'outline';
export type ButtonSize = 'sm' | 'md' | 'lg';
@Component({
  selector: 'app-button',
  imports: [],
  templateUrl: './button.html',
  styleUrl: './button.css',
})
export class Button {
  // Inputs using new signal-based inputs (Angular v17+)
  variant = input<ButtonVariant>('primary');
  size = input<ButtonSize>('md');
  disabled = input<boolean>(false);
  isLoading = input<boolean>(false);
  loadingText = input<string>('Carregando...');
  fullWidth = input<boolean>(false);
  customClasses = input<string>('');

  // Traditional inputs for attributes
  @Input() type: 'button' | 'submit' | 'reset' = 'button';

  // Output events
  clicked = output<void>();

  // Computed properties for classes
  private baseClasses =
    'transition transform active:scale-95 cursor-pointer flex justify-center items-center border border-transparent rounded-md shadow-sm font-medium focus:outline-none focus:ring-2 focus:ring-offset-2 disabled:opacity-50 disabled:cursor-not-allowed';

  private variantClasses = computed(() => {
    const variants = {
      primary:
        'text-black-primary bg-yellow-primary hover:bg-yellow-secondary focus:ring-yellow-primary',
      secondary: 'text-gray-700 bg-gray-200 hover:bg-gray-300 focus:ring-gray-500',
      danger: 'text-white bg-red-600 hover:bg-red-700 focus:ring-red-500',
      success: 'text-white bg-green-600 hover:bg-green-700 focus:ring-green-500',
      outline: 'text-gray-700 bg-transparent border-gray-300 hover:bg-gray-50 focus:ring-gray-500',
    };
    return variants[this.variant()];
  });

  private sizeClasses = computed(() => {
    const sizes = {
      sm: 'py-1 px-3 text-sm',
      md: 'py-2 px-4 text-sm',
      lg: 'py-3 px-6 text-base',
    };
    return sizes[this.size()];
  });

  private widthClasses = computed(() => {
    return this.fullWidth() ? 'w-full' : '';
  });

  buttonClasses = computed(() => {
    return [
      this.baseClasses,
      this.variantClasses(),
      this.sizeClasses(),
      this.widthClasses(),
      this.customClasses(),
    ]
      .filter(Boolean)
      .join(' ');
  });

  handleClick(): void {
    if (!this.disabled() && !this.isLoading()) {
      this.clicked.emit();
    }
  }
}
