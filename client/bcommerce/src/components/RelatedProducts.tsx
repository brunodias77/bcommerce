import React from "react";
import Title from "./Title";

interface RelatedProductsProps {
    category: string;
}

const RelatedProducts: React.FC<RelatedProductsProps> = ({ category }) => {

    return (
        <section className="py-16">
            <Title title="Related" subtitle=" Products" titleStyles="pb-10" />
            {/* CONTAINER */}
            <div className="grid grid-cols-1 xs:grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 gap-8">
            
            </div>

        </section>
    );
};

export default RelatedProducts;