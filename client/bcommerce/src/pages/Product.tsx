import React, { useContext, useEffect, useState } from 'react';
import { useAsyncError, useParams } from 'react-router-dom';
import { ShopContext } from '../context/ShopContext';
import { Product as ProductType } from "../types/product-type";
import { FaCheck, FaHeart, FaStar, FaStarHalfStroke, FaTruckFast } from 'react-icons/fa6';
import { TbShoppingBagPlus } from 'react-icons/tb';
import ProductDescription from '../components/ProductDescription';

const Product: React.FC = () => {
    const { productId } = useParams();
    const { products, currency } = useContext(ShopContext);
    const [product, setProduct] = useState<ProductType | null>(null);
    const [image, setImage] = useState("");
    const [color, setColor] = useState("");

    const fetchProductData = async () => {
        const selectedProduct = products.find((item) => item._id === productId);
        if (selectedProduct) {
            setProduct(selectedProduct);
            setImage(selectedProduct.image[0]);
            console.log(selectedProduct);
        }
    }

    useEffect(() => {
        fetchProductData();
    }, [productId, products])

    if (!product) {
        return <div>Loading...</div>
    }

    return (
        <div>
            <div className='max-padd-container'>
                {/* PRODUCT DATA */}
                <div className="flex gap-10 flex-col xl:flex-row rounded-2xl p-3 mb-6">
                    {/* IMAGE */}
                    <div className='flex flex-1 gap-x-2 max-w-[477px]'>
                        <div className='flex-1 flex items-center justify-center flex-col gap-[7px] flex-wrap cursor-pointer'>
                            {
                                product.image.map((item, i) => (
                                    <img key={i} onClick={() => setImage(item)} src={item} alt="productImg" className="object-cover aspect-square rounded-lg" />
                                ))
                            }
                        </div>
                        <div className="flex-[4] flex">
                            <img src={image} alt="" className="rounded-xl" />
                        </div>
                    </div>
                    {/* PRODUCT DESCRIPTION */}
                    <div className="flex-[1.5] rounded-2xl px-5 py-3 bg-primary">
                        <h3 className="h3 leading-none">{product.name}</h3>
                        <div className='flex items-baseline gap-x-5'>
                            <div className='flex items-center gap-x-2 text-secondary'>
                                <div className='flex gap-x-2 text-secondary'>
                                    <FaStar />
                                    <FaStar />
                                    <FaStar />
                                    <FaStar />
                                    <FaStarHalfStroke />
                                </div>
                                <span className="text-[14px] font-[500]">(122)</span>
                            </div>
                        </div>
                        <h4 className="h4 my-2">{currency}{product.price}.00</h4>
                        <p className="max-w-[555px]">{product.description}</p>

                        {/* COLOR */}
                        <div className="flex flex-ol gap-4 my-4 mb-5">
                            <div className="flex gap-2">
                                {[...product.colors].map((item, i) => (
                                    <button
                                        key={i}
                                        onClick={() => setColor(item)}
                                        className="h-9 w-9 rounded-full flexCenter"
                                        style={{ background: item }}
                                    >
                                        {item === color ? (
                                            <FaCheck
                                                className={item == "White" ? "text-black" : "text-white"}
                                            />
                                        ) : (
                                            <></>
                                        )}

                                    </button>
                                ))}
                            </div>
                        </div>
                        <div className="flex items-center gap-x-4 ">
                            <button className="btn-secondary !rounded-lg sm:w-1/2 flex items-center justify-center gap-x-2 capitalize cursor-pointer hover:brightness-125">
                                Add to Cart <TbShoppingBagPlus />
                            </button>
                            <button onClick={() => { }} className="btn-white !rounded-lg !py-3.5">
                                <FaHeart />
                            </button>
                        </div>
                        <div className="flex items-center gap-x-2 mt-3">
                            <FaTruckFast className="text-lg" /> <span className="medium-14">Free Delivery on order over 500 {currency}</span>
                        </div>
                        <hr className="my-3 w-2/3" />
                        <div className="mt-2 flex flex-col gap-1 text-gray-30 text-[14px]">
                            <p>Authenticy You Can Trust</p>
                            <p>Enjoy ash on Delivery for Your convenience</p>
                            <p>Easy Returns and Exchanges Within 7 Days</p>
                        </div>
                    </div>
                </div>
                <ProductDescription />
                {/* <ProductDescription />
                <ProductFeatures />
                <RelatedProducts category={product.category} /> */}
            </div>
        </div>
    );
};

export default Product;