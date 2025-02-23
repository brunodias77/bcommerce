import React, { useContext, useEffect, useState } from 'react';
import { useAsyncError, useParams } from 'react-router-dom';
import { ShopContext } from '../context/ShopContext';
import { Product as ProductType } from "../types/product-type";

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

    return (
        <div>
            {/* PRODUCT DATA */}
            <div>
                {/* IMAGE */}
                <div>
                    <div>
                        {product && product.image.map((img, index) => (
                            <img key={index} src={img} alt={product.name} />
                        ))}
                    </div>
                </div>
            </div>
        </div>
    );
};

export default Product;