/* eslint-disable @typescript-eslint/no-unused-vars */
import React, { useContext, useEffect, useState } from "react";
import Title from "./Title";
import { ShopContext } from "../context/ShopContext";
import Item from "./Item";
import { Product } from "../types/product-type";

interface RelatedProductsProps {
    category: string;
}

const RelatedProducts: React.FC<RelatedProductsProps> = ({ category }) => {
    const [relatedProducts, setRelatedProducts] = useState<Product[]>([]);
    const { products } = useContext(ShopContext) as { products: Product[] };

    useEffect(() => {
        if (products.length > 0) {
            const filtered = products.filter((item) => category === item.category);
            setRelatedProducts(filtered.slice(0, 5));
        }
    }, [products, category]);

    return (
        <section className="py-16">
            <Title title="Related" subtitle=" Products" titleStyles="pb-10" />
            {/* CONTAINER */}
            <div className="grid grid-cols-1 xs:grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 gap-8">
                {relatedProducts.map((product) => (
                    <div key={product._id}>
                        <Item {...product} />
                    </div>
                ))}
            </div>
        </section>
    );
};

export default RelatedProducts;