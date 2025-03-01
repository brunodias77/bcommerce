/* eslint-disable @typescript-eslint/no-unused-vars */
import React, { useContext, useDebugValue, useEffect, useMemo, useState } from 'react'
import { ShopContext } from '../context/ShopContext'
import { Product } from '../types/product-type';
import Title from '../components/Title';
import { FaRegWindowClose } from 'react-icons/fa';
import { FaMinus, FaPlus, } from 'react-icons/fa6';
import CartTotal from '../components/CartTotal';

interface ProductData {
    _id: string;
    color: string;
    quantity: number;
}

const Cart: React.FC = () => {
    const { products, cartItems, getCartCount, currency } = useContext(ShopContext);
    const [cartData, setCartData] = useState<ProductData[]>([]);
    const [quantity, setQuantity] = useState<Record<string, number>>({});

    // 🚀 Atualiza o cartData já na primeira renderização e quando `cartItems` ou `products` mudam
    useEffect(() => {
        if (products.length === 0 || Object.keys(cartItems).length === 0) {
            setCartData([]);
            setQuantity({});
            return;
        }

        const tempData: ProductData[] = [];
        const initialQuantities: Record<string, number> = {};

        for (const productId in cartItems) {
            for (const color in cartItems[productId]) {
                if (cartItems[productId][color] > 0) {
                    tempData.push({
                        _id: productId,
                        color,
                        quantity: cartItems[productId][color],
                    });
                }
                initialQuantities[`${productId}-${color}`] = cartItems[productId][color];
            }
        }

        setCartData(tempData);
        setQuantity(initialQuantities);
    }, [cartItems, products]); // 🛠 Garante atualização imediata

    return (
        <section >
            <div className='bg-primary mb-16'>
                <div className='max-padd-container py-10'>
                    {/* TITLE */}
                    <div className='flex items-center justify-start gap-x-4'>
                        <Title title='Cart' subtitle=' List' titleStyles='h3' />
                        <h5 className='text-[15px] font-[500] text-gray-30 relative bottom-1.5'>({getCartCount()} Items)</h5>
                    </div>
                    {/* CONTAINER */}
                    <div className='mt-6'>
                        {cartData.map((item, index) => {
                            const productData = products.find((product) => product._id === item._id);
                            const key = `${item._id}-${item.color}`;
                            return (
                                <div key={key} className='rounded-lg bg-white mb-3 p-2'>
                                    <div className='flex items-center gap-x-3'>
                                        <div className='flex items-start gap-6'>
                                            <img src={productData?.image[0]} alt="imagem do produto no carrinho de compras" className='w-20 sm:w-18 rounded' />
                                        </div>
                                        <div className='flex flex-col w-full'>
                                            <div className='flex items-center justify-between'>
                                                <h5 className='h5 !my-0 line-clamp-1'>{productData?.name}</h5>
                                                <FaRegWindowClose className='cursor-pointer text-secondary' />
                                            </div>
                                            <p className='text-[14px] font-[700] my-0.5'>{item.color}</p>
                                            <div className='flex items-center justify-between'>
                                                <div className='flex items-center ring-1 ring-slate-900/5 rounded-full overflow-hidden bg-primary'>
                                                    <button className='p-1.5 bg-white text-secondary rounded-full shadow-md'>
                                                        <FaMinus className='text-xs cursor-pointer' />
                                                    </button>
                                                    <p className='px-3'>{quantity[key]}</p>
                                                    <button className='p-1.5 bg-white text-secondary rounded-full shadow-md'>
                                                        <FaPlus className='text-xs cursor-pointer' />
                                                    </button>
                                                </div>
                                                <h4 className='h4'>
                                                    {currency}{productData?.price}
                                                </h4>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            )
                        })}
                    </div>

                    <div className='flex my-20'>
                        <div className='w-full sm:w-[450px]'>
                            <CartTotal />
                            <button className='btn-secondary mt-7'>Proceed to checkout</button>
                        </div>
                    </div>
                </div>
            </div>


        </section>
    );
};

export default Cart;

