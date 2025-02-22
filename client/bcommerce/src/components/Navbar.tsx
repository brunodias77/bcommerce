import React from 'react';
import { NavLink } from 'react-router-dom';

// Definição das props esperadas pelo componente Navbar
interface NavbarProps {
    containerStyles?: string; // Estilos CSS como string
    onClick?: () => void; // Função opcional para eventos de clique
}

const Navbar: React.FC<NavbarProps> = ({ containerStyles = "", onClick }) => {
    const navLinks = [
        { path: '/', title: 'Home' },
        { path: '/collection', title: 'Collection' },
        { path: '/blog', title: 'Blog' },
        { path: 'mailto:brunohenriqueadias@gmail.com', title: 'Contact' }, // Corrigido mailto:
    ];

    return (
        <nav className={containerStyles}>
            {navLinks.map((link) => (
                <NavLink
                    key={link.title}
                    to={link.path}
                    className={({ isActive }) =>
                        `${isActive ? "active-link" : ""} px-3 py-2 rounded-full`
                    }
                    onClick={onClick}
                >
                    {link.title}
                </NavLink>
            ))}
        </nav>
    );
};

export default Navbar;
