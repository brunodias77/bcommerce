import { useContext } from "react";
import { ShopContext } from "../context/ShopContext";
import PaymentSummaryCard from "./PaymentSummaryCard";

const Summary = () => {
    const { cartItems } = useContext(ShopContext);
    
    return (
        <div className="m-4 w-full mx-auto">
            <div className="shadow-lg rounded-xl bg-white p-4 mt-12 ">
                <h2 className="text-lg font-semibold mb-4">Resumo do Pedido</h2>
                <div className="space-y-4">
                    {Object.entries(cartItems).map(([productId, colors]) => (
                        <PaymentSummaryCard key={productId} productId={productId} />
                    ))}
                </div>
                <h2 className="text-lg font-semibold my-4">Endereco de entrega</h2>
            </div>
        </div>
    );
}

export default Summary;
