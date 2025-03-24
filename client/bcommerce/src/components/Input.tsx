import React from 'react';

type InputProps = {
  label?: string;
  icon?: React.ReactNode;
  inputClassName?: string;
  wrapperClassName?: string;
} & React.InputHTMLAttributes<HTMLInputElement>;

export const Input: React.FC<InputProps> = ({
  label,
  icon,
  inputClassName = '',
  wrapperClassName = '',
  ...props
}) => {
  return (
    <div className={`flex flex-col gap-1 ${wrapperClassName}`}>
      {label && <label className="text-sm font-medium text-gray-700">{label}</label>}
      <div className={`flex items-center border border-gray-300 rounded-md px-3 py-2 bg-white focus-within:ring-2 focus-within:ring-blue-500`}>
        {icon && <span className="mr-2 text-gray-500">{icon}</span>}
        <input
          className={`w-full outline-none bg-transparent text-gray-800 placeholder-gray-400 ${inputClassName}`}
          {...props}
        />
      </div>
    </div>
  );
};
