import React, { createContext, ReactNode, useState } from "react";
import { products } from "../assets/products/data";
import { Product } from "../types/product-type";
import { useNavigate } from "react-router-dom";

// Definição do tipo do contexto
interface ShopContextType {
    products: Product[];
    search: string;
    setSearch: React.Dispatch<React.SetStateAction<string>>;
    currency: number;
}

// Criando o contexto com valor inicial padrão
export const ShopContext = createContext<ShopContextType>({
    products,
    search: "",
    setSearch: () => { }, // Função vazia apenas para inicialização
    currency: 0,
});

interface ShopContextProviderProps {
    children: ReactNode;
}

const ShopContextProvider: React.FC<ShopContextProviderProps> = ({ children }) => {
    const [search, setSearch] = useState("");
    const navigate = useNavigate();
    const currency = 10;


    const value: ShopContextType = { products, search, setSearch, currency };

    return <ShopContext.Provider value={value}>{children}</ShopContext.Provider>;
};

export default ShopContextProvider;
