import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-footer',
  imports: [CommonModule, RouterModule],
  templateUrl: './footer.html',
  styleUrl: './footer.css',
})
export class Footer {
  currentYear = new Date().getFullYear();

  // Links úteis organizados por categoria
  companyLinks = [
    { name: 'Sobre Nós', route: '/sobre' },
    { name: 'Nossa História', route: '/historia' },
    { name: 'Trabalhe Conosco', route: '/carreiras' },
    { name: 'Imprensa', route: '/imprensa' },
  ];

  customerLinks = [
    { name: 'Central de Ajuda', route: '/ajuda' },
    { name: 'Fale Conosco', route: '/contato' },
    { name: 'Política de Privacidade', route: '/privacidade' },
    { name: 'Termos de Uso', route: '/termos' },
  ];

  productLinks = [
    { name: 'Todos os Produtos', route: '/produtos' },
    { name: 'Categorias', route: '/categorias' },
    { name: 'Ofertas', route: '/ofertas' },
    { name: 'Lançamentos', route: '/lancamentos' },
  ];

  socialLinks = [
    { name: 'Facebook', url: 'https://facebook.com/dcommerce', icon: 'facebook' },
    { name: 'Instagram', url: 'https://instagram.com/dcommerce', icon: 'instagram' },
    { name: 'Twitter', url: 'https://twitter.com/dcommerce', icon: 'twitter' },
    { name: 'LinkedIn', url: 'https://linkedin.com/company/dcommerce', icon: 'linkedin' },
  ];

  // Método para scroll suave até o topo
  scrollToTop() {
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }
}
