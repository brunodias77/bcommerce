import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-heart-icon',
  imports: [],
  templateUrl: './heart-icon.html',
  styleUrl: './heart-icon.css',
})
export class HeartIcon {
  @Input() width: number = 22;
  @Input() height: number = 18;
  @Input() color: string = '#191C1F';
}
