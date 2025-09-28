import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-user-icon',
  imports: [],
  templateUrl: './user-icon.html',
  styleUrl: './user-icon.css'
})
export class UserIcon {
  /**
   * @Input() permite que esta propriedade receba um valor do componente pai.
   * Se nenhum valor for passado, o valor padrão (24) será usado.
   */
  @Input() width: number = 24;
  
  @Input() height: number = 24;
  
  @Input() color: string = '#111827';
}
