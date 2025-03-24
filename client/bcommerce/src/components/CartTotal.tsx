/* eslint-disable @typescript-eslint/no-unused-vars */
import React, { useContext } from 'react';
import { ShopContext } from "../context/ShopContext"
import Title from './Title';
import Separator from './Separator';
import Cupom from './Cupom';

const CartTotal: React.FC = () => {
    const { currency, getCartAmount, delivery_charges, cupomDesconto } = useContext(ShopContext);

    const subtotal = getCartAmount();
    const desconto = subtotal * cupomDesconto;
    const total = subtotal - desconto + (subtotal === 0 ? 0 : delivery_charges);

    return (
        <section className='w-full'>
            <Title title='Sub' subtitle=' Total' titleStyles='h3 ' />
            <Cupom />
            <div className='flex item-center justify-between pt-3 '>
                <h5 >SubTotal:</h5>
                <p >{currency}{subtotal.toFixed(2)}</p>
            </div>
            <Separator />
            <div className='flex item-center justify-between pt-3 '>
                <h5 >Cupom de desconto:</h5>
                <p>{cupomDesconto > 0 ? `-${currency}${desconto.toFixed(2)}` : '-'}</p>
            </div>
            <Separator />
            <div className='flex items-center justify-between pt-3'>
                <h5>Frete:</h5>
                <p>{getCartAmount() === 0 ? "0.00" : `${currency}${delivery_charges}.00`}</p>
            </div>
            <Separator />
            <div className='flex items-center justify-between pt-3'>
                <h5 className='text-primary'>Total:</h5>
                <p>{currency}{total.toFixed(2)}</p>
            </div>
        </section>
    );
}

export default CartTotal;