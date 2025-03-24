import React from "react";

interface ButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
    label?: string;
    variant?: 'primary' | 'secondary';
    size?: 'small' | 'medium' | 'large';
    fullWidth?: boolean;
    children?: React.ReactNode;
}

const Button: React.FC<ButtonProps> = ({
    children,
    label,
    variant = 'primary',
    size = 'medium',
    fullWidth = false,
    className = '',
    disabled,
    ...buttonProps
}) => {
    const baseClasses = "py-1 px-2 rounded focus:outline-none transition transform active:scale-95 flex items-center justify-center";
    const variantClasses = {
        primary: "bg-[#f4f4f7] text-white hover:brightness-90",
        secondary: "bg-[#FEC857] text-black  hover:bg-gray-600",
    };
    const sizeClasses = {
        small: "text-sm",
        medium: "text-base",
        large: "text-lg",
    };

    return (
        <button
            className={`
                ${baseClasses} 
                ${variantClasses[variant]} 
                ${sizeClasses[size]} 
                ${fullWidth ? "w-full" : ""} 
                ${disabled ? "opacity-50 cursor-not-allowed" : ""} 
                ${className}
            `}
            disabled={disabled}
            aria-label={label}
            {...buttonProps}
        >
            {children}
        </button>
    );
};

export default Button;
