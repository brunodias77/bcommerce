import React, { useState } from "react";
import { Link } from "react-router-dom";
import { Product } from "../types/product-type";

const Item: React.FC<Product> = ({ _id, image, name, description, price, category }) => {
    const [hovered, setHovered] = useState(false);
    return (
        <div className="overflow-hidden">
            <Link
                to={`/product/${_id}`}
                onMouseEnter={() => setHovered(true)}
                onMouseLeave={() => setHovered(false)}
                className="flex items-center justify-center p-2 bg-[#f5f5f5] overflow-hidden relative"
            >
                <img src={image.length > 1 && hovered ? image[1] : image[0]} alt="Product" className="transition-all duration-300" />            </Link>
            {/* INFO */}
            <div className="p-3 ">
                <h4 className="text-[18px] font-[700] line-clamp-1 !py-0">{name}</h4>
                <div className="flex items-center justify-between pt-1">
                    <p className="text-[14px] md:text-[15px] mb-1 font-bold">{category}</p>
                    <h5 className="text-[14px] md:text-[15px] mb-1 font-bold pr-2">${price}.00</h5>
                </div>
                <p className="line-clamp-2 py-2">{description}</p>
            </div>
        </div >
    );
};

export default Item;