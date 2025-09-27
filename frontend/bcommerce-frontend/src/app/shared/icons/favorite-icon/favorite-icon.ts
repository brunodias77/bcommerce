import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-favorite-icon',
  imports: [],
  templateUrl: './favorite-icon.html',
  styleUrl: './favorite-icon.css',
})
export class FavoriteIcon {
  /**
   * Define se o ícone deve ser exibido como favorito.
   * A opção `{ required: true }` garante que o componente pai
   * sempre precise passar um valor, ex: <app-favorite-icon [isFavorite]="true"></app-favorite-icon>
   */
  @Input({ required: true }) isFavorite!: boolean;

  @Input() width: number = 20;

  @Input() height: number = 20;

  /** Cor de preenchimento e borda quando `isFavorite` é `true` */
  @Input() color: string = '#2d2926';

  /** Cor da borda quando `isFavorite` é `false` */
  @Input() strokeColor: string = '#777777';
}
