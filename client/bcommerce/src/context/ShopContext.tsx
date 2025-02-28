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

export const ShopContext = createContext<ShopContextType | undefined>(undefined);

const ShopContextProvider: React.FC<ShopContextProviderProps> = ({ children }) => {
    const [search, setSearch] = React.useState("");
    const [cartItems, dispatch] = useReducer(cartReducer, initialCartState);
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

    // 🎯 Log do carrinho para depuração
    useEffect(() => {
        console.log("Carrinho atualizado:", cartItems);
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
    };
    return <ShopContext.Provider value={value}>{children}</ShopContext.Provider>;
};
export { ShopContextProvider };

// /* eslint-disable prefer-const */
// /* eslint-disable @typescript-eslint/no-unused-vars */
// import React, { createContext, ReactNode, useEffect, useState } from "react";
// import { products } from "../assets/products/data";
// import { Product } from "../types/product-type";
// import { useNavigate } from "react-router-dom";
// import { toast } from "react-toastify";

// export interface ShopContextType {
//     products: Product[];
//     search: string;
//     setSearch: React.Dispatch<React.SetStateAction<string>>;
//     currency: string;
//     delivery_charges: number;
//     cartItems: CartItems;
//     setCartItems: React.Dispatch<React.SetStateAction<CartItems>>;
//     addToCart: (itemId: string, color: string) => void;
//     getCartCount: () => number;
//     updateQuantities: (itemId: string, color: string, quantity: number) => void;
//     getCartAmount: () => number;
//     navigate: ReturnType<typeof useNavigate>;
// }

// export interface CartItems {
//     [itemId: string]: {
//         [color: string]: number;
//     };
// }


// export const ShopContext = createContext<ShopContextType | undefined>(undefined);


// interface ShopContextProviderProps {
//     children: ReactNode;
// }

// const ShopContextProvider: React.FC<ShopContextProviderProps> = ({ children }) => {
//     const [search, setSearch] = useState("");
//     const [cartItems, setCartItems] = useState<CartItems>({});
//     const navigate = useNavigate();
//     const currency = "$";
//     const delivery_charges = 10;

//     const addToCart = async (itemId: string, color: string) => {

//         if (!color) {
//             toast.error("Vui lòng chọn màu!");
//             return;
//         }

//         let cartData = structuredClone(cartItems);

//         if (cartData[itemId]) {
//             if (cartData[itemId][color]) {
//                 cartData[itemId][color] += 1;
//             } else {
//                 cartData[itemId][color] = 1;
//             }
//         } else {
//             cartData[itemId] = {};
//             cartData[itemId][color] = 1;
//         }

//         setCartItems(cartData);
//     }

//     // Getting total cart count
//     const getCartCount = () => {
//         let totalCount = 0;

//         for (const items in cartItems) {
//             for (const item in cartItems[items]) {
//                 try {
//                     if (cartItems[items][item] > 0) {
//                         totalCount += cartItems[items][item];
//                     }
//                 } catch (error) {
//                     console.log(error);
//                 }
//             }
//         }

//         return totalCount;
//     }

//     const updateQuantities = (itemId: string, color: string, quantity: number) => {
//         let cartData = structuredClone(cartItems);

//         // Kiểm tra và khởi tạo nếu chưa tồn tại
//         if (!cartData[itemId]) {
//             cartData[itemId] = {};
//         }

//         cartData[itemId][color] = quantity;
//         setCartItems(cartData);
//     }

//     const getCartAmount = () => {
//         let totalAmount = 0;
//         for (const items in cartItems) {
//             let itemInfo = products.find((product) => product._id === items)

//             for (const item in cartItems[items]) {
//                 try {
//                     if (cartItems[items][item] > 0) {
//                         totalAmount += itemInfo!.price * cartItems[items][item];
//                     }
//                 } catch (error) {
//                     console.log(error);
//                 }
//             }
//         }
//         return totalAmount;
//     }


//     useEffect(() => {
//         console.log(cartItems);
//     }, [cartItems]);

//     const value: ShopContextType = {
//         products,
//         search,
//         setSearch,
//         currency,
//         delivery_charges,
//         cartItems,
//         setCartItems,
//         addToCart,
//         getCartCount,
//         updateQuantities,
//         getCartAmount,
//         navigate,
//     };

//     return <ShopContext.Provider value={value}>{children}</ShopContext.Provider>;
// };

// export default ShopContextProvider;



