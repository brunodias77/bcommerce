import React, { createContext, ReactNode, useState } from "react";
import { products } from "../assets/products/data";
import { Product } from "../types/product-type";

// Definição do tipo do contexto
interface ShopContextType {
    products: Product[];
    search: string;
    setSearch: React.Dispatch<React.SetStateAction<string>>;
}

// Criando o contexto com valor inicial padrão
export const ShopContext = createContext<ShopContextType>({
    products,
    search: "",
    setSearch: () => { } // Função vazia apenas para inicialização
});

interface ShopContextProviderProps {
    children: ReactNode;
}

const ShopContextProvider: React.FC<ShopContextProviderProps> = ({ children }) => {
    const [search, setSearch] = useState("");

    const value: ShopContextType = { products, search, setSearch };

    return <ShopContext.Provider value={value}>{children}</ShopContext.Provider>;
};

export default ShopContextProvider;
