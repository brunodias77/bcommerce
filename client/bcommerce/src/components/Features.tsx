import React from 'react'
import { BiSupport } from 'react-icons/bi';
import { RiMoneyDollarCircleLine } from 'react-icons/ri';
import { TbTruckDelivery } from 'react-icons/tb';

const Features: React.FC = () => {
    return (
        <section className='max-padd-container mt-16'>
            {/* CONTAINER */}
            <div className='max-padd-container flex items-center justify-between flex-wrap gap-8 rounded-2xl'>
                <div className='flex items-center justify-center gap-x-3'>
                    <RiMoneyDollarCircleLine className='text-3xl ' />
                    <div>
                        <h4 className='text-[15px] font-[500] text-gray-800'>MONEY-BACK GUARANTEE</h4>
                        <p className='text-gray-400'>100% refund guaranteed if you're not satisfied.</p>
                    </div>
                </div>
                <div className='flex items-center justify-center gap-x-3'>
                    <TbTruckDelivery className='text-3xl ' />
                    <div>
                        <h4 className='text-[15px] font-[500] text-gray-800'>FREE SHIPPING & RETURNS</h4>
                        <p className='text-gray-400'>Free shipping available on all orders above $99.</p>
                    </div>
                </div>
                <div className='flex items-center justify-center gap-x-3'>
                    <BiSupport className='text-3xl ' />
                    <div>
                        <h4 className='text-[15px] font-[500] text-gray-800'>24/7 ONLINE SUPPORT</h4>
                        <p className='text-gray-400'>Our team is here to assist you round the clock..</p>
                    </div>
                </div>
            </div>
        </section >
    )
}

export default Features;
