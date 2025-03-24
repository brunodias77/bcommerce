/* eslint-disable prefer-const */
/* eslint-disable @typescript-eslint/no-unused-vars */
import React, { createContext, ReactNode, useEffect, useReducer, useMemo } from "react";
import { products } from "../assets/products/data";
import { Product } from "../types/product-type";
import { useNavigate } from "react-router-dom";
import { toast } from "react-toastify";
import { CartItems } from "../types/cart-type";

interface ShopContextProviderProps {
    children: ReactNode;
}
export interface ShopContextType {
    products: Product[];
    search: string;
    setSearch: React.Dispatch<React.SetStateAction<string>>;
    currency: string;
    delivery_charges: number;
    cartItems: CartItems;
    addToCart: (itemId: string, color: string) => void;
    updateQuantities: (itemId: string, color: string, quantity: number) => void;
    getCartCount: () => number;
    getCartAmount: () => number;
    navigate: ReturnType<typeof useNavigate>;
    aplicarCupom: (codigo: string) => boolean;
    cupomDesconto: number;
}

type CartAction =
    | { type: "ADD_TO_CART"; itemId: string; color: string }
    | { type: "UPDATE_QUANTITY"; itemId: string; color: string; quantity: number };

const initialCartState: CartItems = {};

const cartReducer = (state: CartItems, action: CartAction): CartItems => {
    const newState = JSON.parse(JSON.stringify(state)); // Clonagem segura do estado
    switch (action.type) {
        case "ADD_TO_CART":
            if (!action.color) {
                toast.error("Selecione uma cor!");
                return state;
            }

            if (!newState[action.itemId]) {
                newState[action.itemId] = {};
            }

            newState[action.itemId][action.color] = (newState[action.itemId][action.color] || 0) + 1;
            return newState;

        case "UPDATE_QUANTITY":
            if (!newState[action.itemId]) {
                newState[action.itemId] = {};
            }

            newState[action.itemId][action.color] = action.quantity;
            return newState;

        default:
            return state;
    }
};


export const ShopContext = createContext<ShopContextType>({} as ShopContextType);

const ShopContextProvider: React.FC<ShopContextProviderProps> = ({ children }) => {
    const [search, setSearch] = React.useState("");
    const [cartItems, dispatch] = useReducer(cartReducer, initialCartState);
    const [cupomDesconto, setCupomDesconto] = React.useState<number>(0);

    const navigate = useNavigate();
    const currency = "$";
    const delivery_charges = 10;

    // 🎯 Adicionar ao carrinho
    const addToCart = (itemId: string, color: string) => {
        dispatch({ type: "ADD_TO_CART", itemId, color });
    };

    // 🎯 Atualizar quantidade
    const updateQuantities = (itemId: string, color: string, quantity: number) => {
        dispatch({ type: "UPDATE_QUANTITY", itemId, color, quantity });
    };

    // 🎯 Contar total de itens no carrinho
    const getCartCount = useMemo(() => {
        return () => Object.values(cartItems).reduce(
            (acc, colors) => acc + Object.values(colors).reduce((sum, qty) => sum + qty, 0),
            0
        );
    }, [cartItems]);

    // 🎯 Calcular total do carrinho
    const getCartAmount = useMemo(() => {
        return () => Object.entries(cartItems).reduce((total, [itemId, colors]) => {
            const item = products.find((product) => product._id === itemId);
            if (!item) return total;

            const itemTotal = Object.values(colors).reduce((sum, qty) => sum + qty * item.price, 0);
            return total + itemTotal;
        }, 0);
    }, [cartItems]);

    const aplicarCupom = (codigo: string) => {
        if (codigo.toLowerCase() === 'desconto10') {
            setCupomDesconto(0.1); // 10%
            toast.success('Cupom aplicado com sucesso! ✅');
            return true;
        } else {
            setCupomDesconto(0);
            toast.error('Cupom inválido! ❌');
            return false;
        }
    };

    // 🎯 Log do carrinho para depuração
    useEffect(() => {
        console.log("Carrinho atualizado no context:", cartItems);
    }, [cartItems]);

    const value: ShopContextType = {
        products,
        search,
        setSearch,
        currency,
        delivery_charges,
        cartItems,
        addToCart,
        updateQuantities,
        getCartCount,
        getCartAmount,
        navigate,
        aplicarCupom,
        cupomDesconto
    };
    return <ShopContext.Provider value={value}>{children}</ShopContext.Provider>;
};
export { ShopContextProvider };




/*
No código que você forneceu, o useMemo e o useReducer desempenham papéis importantes para a otimização de desempenho e para a organização do estado e das ações no contexto de uma loja virtual. Vamos entender como cada um deles é utilizado:

1. useReducer
O useReducer é um hook do React que gerencia o estado do componente de maneira mais complexa, ideal para cenários onde o estado é um objeto ou array complexo e você tem diferentes tipos de ações que podem modificar esse estado.

No seu código, o useReducer é usado para gerenciar o estado do carrinho de compras (cartItems):


const [cartItems, dispatch] = useReducer(cartReducer, initialCartState);
Estado: cartItems guarda o estado do carrinho de compras. Ele começa com o valor de initialCartState, que é um objeto vazio ({}).
Ação: dispatch é uma função que dispara ações para modificar o estado do carrinho.
Redutor: cartReducer é a função que define como o estado (o carrinho) deve ser alterado em resposta às ações. As ações possíveis são:
"ADD_TO_CART": Adiciona um item ao carrinho (se a cor for selecionada).
"UPDATE_QUANTITY": Atualiza a quantidade de um item no carrinho.
A vantagem de usar useReducer aqui é que ele facilita o controle de um estado mais complexo (o carrinho) e ajuda a manter a lógica de alterações do estado centralizada na função cartReducer, tornando o código mais modular e fácil de manter.

2. useMemo
O useMemo é usado para memorizar um valor calculado, ou seja, ele evita que o cálculo de um valor seja repetido a cada renderização, a menos que uma das dependências mude. Isso pode melhorar o desempenho, especialmente se o cálculo for caro (ex.: fazer uma iteração em uma lista grande).

No seu código, o useMemo é utilizado para calcular o número total de itens no carrinho (getCartCount) e o valor total do carrinho (getCartAmount), mas apenas quando o estado de cartItems mudar:

getCartCount:
javascript
Copiar
const getCartCount = useMemo(() => {
    return () => Object.values(cartItems).reduce(
        (acc, colors) => acc + Object.values(colors).reduce((sum, qty) => sum + qty, 0),
        0
    );
}, [cartItems]);
Este cálculo retorna o número total de itens no carrinho, somando as quantidades de cada cor de cada item.
O cálculo só é feito novamente quando o estado cartItems muda (esse estado é uma dependência do useMemo).
getCartAmount:
javascript
Copiar
const getCartAmount = useMemo(() => {
    return () => Object.entries(cartItems).reduce((total, [itemId, colors]) => {
        const item = products.find((product) => product._id === itemId);
        if (!item) return total;

        const itemTotal = Object.values(colors).reduce((sum, qty) => sum + qty * item.price, 0);
        return total + itemTotal;
    }, 0);
}, [cartItems]);
Este cálculo retorna o valor total do carrinho, multiplicando as quantidades de cada item pela respectiva price.
Ele também depende de cartItems, então o valor é recalculado apenas quando esse estado mudar.
Resumo da utilização de useMemo e useReducer:
useReducer: Utilizado para gerenciar um estado complexo (cartItems), facilitando o controle das alterações do carrinho (adicionar itens, atualizar quantidades).

useMemo: Utilizado para memorizar os cálculos do número total de itens (getCartCount) e o valor total do carrinho (getCartAmount), evitando recomputações desnecessárias a cada renderização. A recomputação só ocorre quando cartItems é alterado.

Essa combinação ajuda a melhorar a eficiência do código, principalmente quando se trata de cálculos que podem ser pesados ou quando o estado é complexo e pode ter muitos tipos diferentes de modificações.
*/