import React from 'react'
import headphones from "../assets/products/headphones.png";
import { FaArrowRightLong } from 'react-icons/fa6';

const Hero: React.FC = () => {
    return (
        <section className='mx-auto max-w-[1440px] px-6 lg:px-12'>
            <div className='grid grid-cols-2 bg-hero bg-cover bg-center bg-no-repeat rounded-2xl h-[633px]'>
                {/* LEFT SIDE */}
                <div className='place-content-end max-xs:min-w-80'>
                    <div className="p-4">
                        <p className='text-white max-w-xs'>Lorem ipsum dolor sit, amet consectetur adipisicing elit. Similique voluptatibus expedita doloremque dignissimos. Facere minima nihil eaque dolorem, sapiente beatae inventore, maiores nisi, provident unde obcaecati ipsam assumenda rem accusantium?</p>
                        <button className="bg-white text-tertiary ring-1 ring-white px-7 py-2.5 rounded-full mt-6">Explore more</button>
                    </div>
                </div>

                {/* RIGHT SIDE */}
                <div className='hidden xs:block place-items-end'>
                    <div className='flex flex-col rounded-2xl w-[211px] relative top-10 right-4 p-2 bg-white'>
                        <img src={headphones} alt="" className='rounded-2xl bg-slate-900/5' />
                        <button className=' bg-primary ring-1 ring-primary px-7 py-2.5 rounded-full transition-all duration-300 !py-1 !text-xs flex items-center justify-center gap-2 mt-2'>Explore this product<FaArrowRightLong /></button>
                    </div>
                </div>
            </div>
        </section>
    )
}

export default Hero
