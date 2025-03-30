import React from 'react';
import { Address } from '../types/address-type';
import { DeleteIcon, EditIcon } from './StepIcons';
import LoadingSpinner from './LoadingSpinner';

interface AddressCardProps {
    data: Address;
    checked: boolean;
    onChange: (id: string) => void;
}

const AddressCard: React.FC<AddressCardProps> = ({ data, checked, onChange }) => {
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
                    <span className="text-sm font-bold text-primary">{data.street}</span>
                </div>
                <div className="text-sm text-gray-500">
                    {data.city} - {data.state}, {data.zipCode}
                </div>
                <div className="text-xs text-gray-400">
                    {data.contactName} - {data.phoneNumber}
                </div>
            </div>
            <div id='icons' className='flex items-center justify-center mt-2 gap-x-8'>
                <EditIcon />
                <DeleteIcon />
            </div>

        </label>
    );
};

export default AddressCard;

