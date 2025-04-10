
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
    const baseClasses = " rounded cursor-pointer focus:outline-none transition transform active:scale-95 flex items-center justify-center";
    const variantClasses = {
        primary: "bg-[#2D2926] text-white hover:brightness-50",
        secondary: "bg-[#FEC857] text-black  hover:brightness-90",
    };
    const sizeClasses = {
        small: "py-1 px-2 ",
        medium: "py-2 px-4 ",
        large: "py-4 px-8 ",
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
