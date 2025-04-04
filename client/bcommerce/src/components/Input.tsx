import React from 'react';

interface InputProps extends React.InputHTMLAttributes<HTMLInputElement> {
    id: string;
    label: string;
}

const Input: React.FC<InputProps> = ({ id, label, ...props }) => {
    return (
        <div>
            <label htmlFor={id} className="block text-sm font-medium text-gray-700 mb-2">
                {label}
            </label>
            <input
                id={id}
                {...props} // 👈 Repassa tudo (name, type, placeholder, required, etc)
                className="w-full p-2.5 text-sm text-gray-900 border border-gray-300 rounded-lg bg-white focus:ring-blue-500 focus:border-blue-500"
            />
        </div>
    );
};

export default Input;


