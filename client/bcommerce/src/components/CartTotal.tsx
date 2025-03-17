/* eslint-disable @typescript-eslint/no-unused-vars */
import React, { useContext } from 'react';
import { ShopContext } from "../context/ShopContext"
import Title from './Title';

const CartTotal: React.FC = () => {
    const { currency, getCartAmount, delivery_charges } = useContext(ShopContext);
    return (
        <section className='w-full'>
            <Title title='Cart' subtitle=' Total' titleStyles='h3' />
            <div className='flex item-center justify-between '>
                <h5 className='h5'>SubTotal:</h5>
                <p className='h5'>{currency}{getCartAmount()}.00</p>
            </div>
            <hr className='mx-auto h-[1px] w-full bg-gray-900/90 my-1' />
            <div className='flex items-center justify-between pt-3'>
                <h5>Shipping Free</h5>
                <p>{getCartAmount() === 0 ? "0.00" : `${currency}${delivery_charges}.00`}</p>
            </div>
            <hr className='mx-auto h-[1px] w-full bg-gray-900/90 my-1' />
            <div className='flex items-center justify-between pt-3'>
                <h5>Total:</h5>
                <p>{getCartAmount() === 0 ? "0.00" : getCartAmount() + delivery_charges}</p>
            </div>
        </section>
    );
}

export default CartTotal;