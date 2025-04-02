import React from 'react';
import { Shipping } from '../types/shipping-type';

interface ShippingCardProps {
    data: Shipping;
    checked: boolean;
    onChange: (id: string) => void;
}


const ShippingCard: React.FC<ShippingCardProps> = ({ checked, data, onChange }) => {
    return (
        <label
            htmlFor={data.id}
            className={` p-4 bg-white shadow-md rounded-lg flex items-center justify-between  cursor-pointer border transition-colors ${checked ? 'border-yellow-primary' : 'border-transparent hover:border-gray-300'
                }`}
        >
            <div className='flex flex-col gap-y-1'>
                <div className="flex items-center gap-x-2">
                    <input
                        type="radio"
                        name="selectedAddress"
                        id={data.id}
                        checked={checked}
                        onChange={() => onChange(data.id)}
                        className={`
                             w-4 h-4 rounded-full border-2 
                             ${checked ? 'bg-yellow-primary border-yellow-primary' : 'border-gray-400'}
                             appearance-none
                             cursor-pointer
                             transition-colors duration-200
                           `} />
                    <span className='font-bold text-primary'>{data.name}</span>
                </div>
                <span className='text-gray-400'>{data.description}</span>
            </div>
            <span className='text-gray-400'>{data.estimatedDelivery}</span>
        </label>
    );
};

export default ShippingCard;