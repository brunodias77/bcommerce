/* eslint-disable @typescript-eslint/no-unused-vars */
import React, { use, useContext, useState } from 'react'
import { Link } from 'react-router-dom';
import Navbar from './Navbar';
import { FaBars, FaBarsStaggered } from 'react-icons/fa6';
//import { TbUserCircle } from 'react-icons/tb';
//import { RiUserLine } from 'react-icons/ri';
import { IoCartOutline } from "react-icons/io5";
import { IoPersonOutline } from "react-icons/io5";
import logo from "../assets/logo/logo.svg";
import { ShopContext } from '../context/ShopContext';

const Header: React.FC = () => {
    const [menuOpened, setMenuOpened] = useState(false);
    const shopContext = useContext(ShopContext);

    if (!shopContext) {
        throw new Error("ShopContext deve ser usado dentro de um ShopContextProvider");
    }

    const { getCartCount } = shopContext;

    const toggleMenu = () => setMenuOpened((prev) => !prev);
    return (
        <header className='max-padd-container w-full mb-2'>
            <div className='flex items-center justify-between'>
                {/*LOGO*/}
                <Link to={'/'} className='flex flex-1 '>
                    <img src={logo} alt="logo da empresa" className='max-w-[60px] max-h-[60px] pt-2' />
                </Link>

                {/*NAVBAR*/}
                <div className='flex-1 '>
                    <Navbar
                        containerStyles={`${menuOpened ? "flex items-start flex-col gap-x-8 fixed top-16 right-6 py-5 px-5 bg-white rounded-xl shadow-md w-52 ring-1 ring-slate-900/5 z-50" : "hidden xl:flex items-center justify-around  gap-x-5 xl:gap-x-7 text-[15px] font-[500] bg-primary ring-1 ring-slate-900/5 rounded-full p-1"}`}
                        onClick={() => setMenuOpened(false)}
                    />
                </div>

                {/*BUTTONS*/}
                <div className='flex-1 flex items-center justify-end gap-x-2 xs:gap-x-8'>
                    {/*MENU TOGGLE*/}
                    <>
                        {menuOpened ? (
                            <FaBarsStaggered
                                onClick={toggleMenu}
                                className='xl:hidden cursor-pointer text-xl ' />
                        ) : (
                            <FaBars
                                onClick={toggleMenu}
                                className='xl:hidden cursor-pointer text-xl ' />
                        )}
                    </>
                    {/* CART */}
                    <Link to={'/cart'} className='flex relative'>
                        <IoCartOutline size={20} />
                        <span className='bg-[#FEC857] text-black text-[12px] font-semibold absolute -top-3.5 -right-2 flex items-center justify-center w-4 h-4 rounded-full shadow-md'>
                            {getCartCount()}
                        </span>
                    </Link>
                    {/* USER PROFILE */}
                    <Link to={'/perfil'} className='group relative'>
                        <button className='cursor-pointer flex'>
                            <IoPersonOutline size={20} />
                        </button>
                    </Link>

                </div>
            </div>

        </header>
    )
}

export default Header;
