import React, { useEffect } from 'react'
import Item from './Item';
import Title from './Title';
import { Swiper, SwiperSlide } from 'swiper/react';
import { Autoplay } from 'swiper/modules';
import { Product } from '../types/product-type';
import { ShopContext } from '../context/ShopContext';
import 'swiper/css';

const NewArrivals: React.FC = () => {

    const [PopularProducts, setPopularProducts] = React.useState<Product[]>([]);
    const products = React.useContext(ShopContext);

    useEffect(() => {
        if (!products) return;
        const data = products.products.slice(0, 7);
        setPopularProducts(data);
    }, [products]);

    return (
        <section className='max-padd-container pt-16'>
            <Title title='New' subtitle=' Arrivals' content='Discover the best deals on top-quality products, Crafted
            to elevate your everyday experience.' titleStyles='pb-1' pStyles='!block' styles='!block !pb-10'
            />
            {/* CONTAINER */}
            <Swiper
                autoplay={{
                    delay: 2500,
                    disableOnInteraction: false,
                }}
                breakpoints={{
                    300: {
                        slidesPerView: 2,
                        spaceBetween: 30
                    },
                    670: {
                        slidesPerView: 3,
                        spaceBetween: 30
                    },
                    900: {
                        slidesPerView: 4,
                        spaceBetween: 30
                    },
                    1300: {
                        slidesPerView: 5,
                        spaceBetween: 30
                    }
                }}
                modules={[Autoplay]}
                className="h-[399px] mt-5"
            >
                {PopularProducts.map((product) => (
                    <SwiperSlide key={product._id}><Item {...product} /></SwiperSlide>
                ))}
            </Swiper>


        </section>
    )
}

export default NewArrivals;
