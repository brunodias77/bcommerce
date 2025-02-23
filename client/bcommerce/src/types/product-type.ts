// Definição do tipo para um produto
export interface Product {
  _id: string;
  name: string;
  description: string;
  price: number;
  image: string[];
  category: string;
  colors: string[];
  date: number;
  popular: boolean;
}

// Definição do tipo para um blog
export interface Blog {
  title: string;
  category: string;
  image: string;
}
