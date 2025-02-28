/* eslint-disable prefer-const */
/* eslint-disable @typescript-eslint/no-unused-vars */
import React, { createContext, ReactNode, useEffect, useState } from "react";
import { products } from "../assets/products/data";
import { Product } from "../types/product-type";
import { useNavigate } from "react-router-dom";
import { toast } from "react-toastify";

export interface ShopContextType {
    products: Product[];
    search: string;
    setSearch: React.Dispatch<React.SetStateAction<string>>;
    currency: string;
    //   delivery_charges: number;
    cartItems: CartItems;
    setCartItems: React.Dispatch<React.SetStateAction<CartItems>>;
    addToCart: (itemId: string, color: string) => void;
    //    getCartCount: () => number;
    //   updateQuantities: (itemId: string, color: string, quantity: number) => void;
    //   getCartAmount: () => number;
    navigate: ReturnType<typeof useNavigate>;
}

export interface CartItems {
    [itemId: string]: {
        [color: string]: number;
    };
}


export const ShopContext = createContext<ShopContextType | undefined>(undefined);

interface ShopContextProviderProps {
    children: ReactNode;
}

const ShopContextProvider: React.FC<ShopContextProviderProps> = ({ children }) => {
    const [search, setSearch] = useState("");
    const [cartItems, setCartItems] = useState<CartItems>({});
    const navigate = useNavigate();
    const currency = "$";

    const addToCart = async (itemId: string, color: string) => {
        const cartData = structuredClone(cartItems);
        if (!color) {
            toast.error("Please select a color !");
            return;
        }
        if (cartData[itemId]) {
            if (cartData[itemId][color]) {
                cartData[itemId][color] += 1;
            } else {
                cartData[itemId][color] = 1;
            }
        } else {
            cartData[itemId] = {};
            cartData[itemId][color] = 1;
        }
        setCartItems(cartData);
    };

    // Getting total cart count
    const getCartCount = () => {
        let totalAmount = 0;
        for (const items in cartItems) {
            let itemInfo = products.find((product) => product._id === items)
            for (const item in cartItems[items]) {
                try {
                    if (cartItems[items][item] > 0) {
                        totalAmount += itemInfo.price * cartItems[items][item];
                    }
                } catch (error) {
                    console.log(error);
                }
            }
        }
        return totalAmount;
    }

    useEffect(() => {
        console.log(cartItems);
    }, [cartItems]);

    const value: ShopContextType = {
        products,
        search,
        setSearch,
        currency,
        //       delivery_charges,
        cartItems,
        setCartItems,
        addToCart,
        //       getCartCount,
        //       updateQuantities,
        //       getCartAmount,
        navigate,
    };

    return <ShopContext.Provider value={value}>{children}</ShopContext.Provider>;
};

export default ShopContextProvider;
