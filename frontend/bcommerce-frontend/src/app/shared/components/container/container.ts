// container.component.ts
import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-container',
  standalone: true,
  template: `
    <div [class]="getOuterClasses()">
      <div [class]="getInnerClasses()">
        <ng-content></ng-content>
      </div>
    </div>
  `,
  styles: [],
})
export class Container {
  @Input() outerClasses: string = '';
  @Input() innerClasses: string = '';

  private defaultOuterClasses = 'min-h-screen w-full';
  private defaultInnerClasses = 'max-w-[1220px] mx-auto px-6 py-8 lg:px-12';

  getOuterClasses(): string {
    return `${this.defaultOuterClasses} ${this.outerClasses}`.trim();
  }

  getInnerClasses(): string {
    return `${this.defaultInnerClasses} ${this.innerClasses}`.trim();
  }
}
