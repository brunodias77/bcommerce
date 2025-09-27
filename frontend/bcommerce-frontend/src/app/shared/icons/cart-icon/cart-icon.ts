import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-cart-icon',
  imports: [],
  templateUrl: './cart-icon.html',
  styleUrl: './cart-icon.css',
})
export class CartIcon {
  // @Input() permite que a propriedade receba um valor de um componente pai.
  // Valores padrão são definidos para cada propriedade.
  @Input() width: number = 21;
  @Input() height: number = 21;
  @Input() color: string = '#191C1F';
  @Input() isActive: boolean = true;
}
