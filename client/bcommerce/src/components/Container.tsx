import React from 'react';

interface ContainerProps extends React.HTMLAttributes<HTMLElement> {
    as?: React.ElementType;
    children: React.ReactNode;
    className?: string;
}

const Container: React.FC<ContainerProps> = ({
    as: Component = 'div',
    children,
    className = '',
    ...rest
}) => {
    const baseClasses = "mx-auto max-w-[1440px] px-6 py-3 lg:px-12";

    return (
        <Component className={`${baseClasses} ${className}`} {...rest}>
            {children}
        </Component>
    );
};

export default Container;


// <Container as="section">...</Container>
// <Container as="main">...</Container>
// <Container as="header" className="bg-gray-100">Header</Container>
