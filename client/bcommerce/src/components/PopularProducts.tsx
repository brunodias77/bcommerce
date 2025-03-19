import React, { useEffect } from 'react'
import Title from './Title'
import { products } from "../assets/products/data"
import { Product } from '../types/product-type';
import Item from './Item';
import ProductCard from './ProductCard';

const PopularProducts = () => {
    const [popularProducts, setPopularProducts] = React.useState<Product[]>([]);
    useEffect(() => {
        const data = products.filter((product) => product.popular);
        setPopularProducts(data.slice(0, 5));
    }, []);
    return (
        <section className='max-padd-container py-16'>
            <Title title='Popular' subtitle=' Products' content='Discover the best deals on top-quality products, Crafted
to elevate your everyday experience.' titleStyles='pb-1' pStyles='!block' styles='!block !pb-10'
            />
            {/*CONTAINER*/}
            <div className='grid grid-cols-1 xs:grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 gap-8 '>
                {popularProducts.map((product) => (
                    <div key={product._id}>
                        <ProductCard {...product} />
                    </div>
                ))}
            </div>
        </section>
    )
}

export default PopularProducts
