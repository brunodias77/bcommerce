import React from 'react'
import { FaDribbble, FaFacebookF, FaInstagram } from "react-icons/fa6";
import { FiFacebook } from "react-icons/fi";
import { SlSocialFacebook } from "react-icons/sl";

const NewLatter: React.FC = () => {

    return (
        <section className='max-padd-container border-t-[1px] border-b-[1px] border-primary py-4'>
            <div className='flex items-center justify-between flex-wrap gap-7'>
                <div>
                    <h4 className='text-[14px] font-[700] uppercase tracking-wider'>Subscribe newsletter</h4>
                    <p>Get latest information on events, sales & offers.</p>
                </div>
                <div>
                    <div className='flex bg-primary'>
                        <input type="email" placeholder='Email Address' className='p-4 bg-primary w-[266px] outline-none text-xs' />
                        <button className=' bg-tertiary text-white ring-1 ring-tertiary px-7 py-2.5 rounded-none !text-[13px] !font-bold uppercase'>Submit</button>
                    </div>
                </div>
                <div className='flex gap-x-3 pr=14'>
                    <div className='h-8 w-8 rounded-full hover:bg-tertiary hover:text-white flex items-center justify-center transition-all duration-300'>
                        <SlSocialFacebook />
                    </div>
                    <div className='h-8 w-8 rounded-full hover:bg-tertiary hover:text-white flex items-center justify-center transition-all duration-300'>
                        <FaInstagram />
                    </div>                   <div className='h-8 w-8 rounded-full hover:bg-tertiary hover:text-white flex items-center justify-center transition-all duration-300'>
                        <FaDribbble />
                    </div>
                </div>
            </div>
        </section>
    )
}

export default NewLatter;
