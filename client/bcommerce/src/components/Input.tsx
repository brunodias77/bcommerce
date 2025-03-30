import React from 'react';

interface InputProps {
    id: string;
    label: string;
    type?: string;
    placeholder?: string;
    required?: boolean;
}

const Input: React.FC<InputProps> = ({
    id,
    label,
    type = 'text',
    placeholder,
    required = false,
}) => {
    return (
        <div>
            <label htmlFor={id} className="block text-sm font-medium text-gray-700 mb-2">
                {label}
            </label>
            <input
                type={type}
                id={id}
                required={required}
                placeholder={placeholder}
                className="w-full p-2.5 text-sm text-gray-900 border border-gray-300 rounded-lg bg-white focus:ring-blue-500 focus:border-blue-500"
            />
        </div>
    );
};

export default Input;
