import React from "react";
import { products } from "../assets/products/data";
import { Product } from "../types/product-type";

interface PaymentSummaryCardProps {
    productId: string;
}

const PaymentSummaryCard: React.FC<PaymentSummaryCardProps> = ({ productId }) => {
    const product: Product | undefined = products.find((p) => p._id === productId);

    if (!product) {
        return (
            <div className="bg-[#f6f6f6] rounded-xl px-2 py-1 text-center">
                <span>Produto não encontrado ❌</span>
            </div>
        );
    }

    return (
        <div className="bg-[#f6f6f6] rounded-xl pl-2 pr-4 py-1 flex items-center justify-between space-x-2">
            <img
                src={product.image[0]}
                alt={product.name}
                className="w-16 h-16 object-cover rounded"
            />
            <span className="block font-semibold">{product.name}</span>
            <span className="block text-gray-600">${product.price.toFixed(2)}</span>
        </div>
    );
};

export default PaymentSummaryCard;
