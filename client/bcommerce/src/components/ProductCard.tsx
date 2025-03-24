import React from "react";
import { Link } from "react-router-dom";
import { Product } from "../types/product-type";
import StarRating from "./StarRating";

const ProductCard: React.FC<Product> = ({ _id, image, name, price, category }) => {
    const oldPrice = price + 10;
    const isOnSale = oldPrice > price;

    return (
        <div className="relative flex flex-col items-center group w-full">
            {/* Card Principal */}
            <div className="relative z-10 overflow-hidden border border-gray-200 rounded-lg shadow-sm transition-transform hover:scale-105 duration-300 w-full">
                {/* Selo de OFERTA */}
                {isOnSale && (
                    <div className="absolute top-2 right-2 bg-yellow-500 text-white text-xs px-2 py-1 rounded">
                        OFERTA
                    </div>
                )}

                {/* Imagem */}
                <Link to={`/product/${_id}`} className="block bg-gray-100 overflow-hidden w-full">
                    <img
                        src={image[0]}
                        alt={name}
                        className="transition-all duration-300 object-cover w-full h-[180px] md:h-[220px] group-hover:opacity-70"
                    />
                </Link>

                {/* INFO */}
                <div className="p-3 w-full">
                    <h2 className="text-[16px] font-bold text-gray-900 line-clamp-1">{name}</h2>
                    <h4 className="text-[12px] md:text-[13px] mb-1 text-gray-400">{category}</h4>

                    {/* Estrelas */}
                    <StarRating size="text-[15px] md:text-[20px]" />

                    {/* Preço e Preço Antigo */}
                    <div className="flex items-center gap-x-2 mt-2">
                        <h5 className="text-[14px] md:text-[15px] font-bold text-gray-900">${price}.00</h5>
                        {isOnSale && (
                            <span className="text-sm md:text-md text-gray-400 line-through">${oldPrice}.00</span>
                        )}
                    </div>
                </div>
            </div>

            <div className="absolute  w-full left-0 bottom-0 transition-all duration-300 translate-y-0 opacity-0 z-[-1] group-hover:translate-y-[120%] group-hover:opacity-100 group-hover:z-0">
                <button className="w-full bg-[#FEC857] hover:bg-yellow-600 text-white font-bold py-2 rounded-md shadow-md cursor-pointer">
                    Adicionar ao Carrinho
                </button>
            </div>
        </div>
    );
};

export default ProductCard;
