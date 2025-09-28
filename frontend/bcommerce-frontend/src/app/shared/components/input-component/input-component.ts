import { Component, Input } from '@angular/core';
import { FormGroup, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-input',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './input-component.html',
  styleUrl: './input-component.css',
})
export class InputComponent {
  @Input() formGroup!: FormGroup; // FormGroup vindo do pai
  @Input() controlName!: string; // Nome do control (ex: "email")
  @Input() label!: string; // Label exibido acima do input
  @Input() type: string = 'text'; // Tipo do input (text, email, password, etc.)
  @Input() placeholder: string = ''; // Placeholder
}
