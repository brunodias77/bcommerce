import { useContext, useState } from "react";
import Container from "../components/Container";
import ShippingCard from "../components/ShippingCard";
import { AddressIcon, PaymentIcon, ShippingIcon } from "../components/StepIcons";
import usePageName from '../hooks/UsePageName';
import { Shipping as ShippingType } from "../types/shipping-type";
import Button from "../components/Button";
import { ShopContext } from "../context/ShopContext";

const Shipping: React.FC = () => {
    const pageName = usePageName();
    const isActive = (name: string) => pageName === name;
    const [selectedShippingId, setSelectedShippingId] = useState<string | null>("shipping1");
    const { navigate } = useContext(ShopContext);

    const shippings: ShippingType[] = [
        {
            id: 'shipping1',
            name: "Sedex",
            price: 20.00,
            estimatedDelivery: "2-5 dias úteis",
            description: "Entrega rápida via Sedex",
            shippingCompany: "Correios",
            shippingTime: "2-5 dias úteis",
        },
        {
            id: 'shipping2',
            name: "PAC",
            price: 10.00,
            estimatedDelivery: "5-10 dias úteis",
            description: "Entrega econômica via PAC",
            shippingCompany: "Correios",
            shippingTime: "5-10 dias úteis",
        },
        {
            id: 'shipping3',
            name: "Transportadora XYZ",
            price: 15.00,
            estimatedDelivery: "3-7 dias úteis",
            description: "Entrega terceirizada com rastreio",
            shippingCompany: "Transportadora XYZ",
            shippingTime: "3-7 dias úteis",
        }
    ];


    return (
        <section className="bg-primary">
            <Container>
                <div id="steps" className="flex items-center justify-between gap-x-4">
                    {/* Passo 1 - Endereço */}
                    <div id="address" className="flex items-center gap-x-2">
                        <div>
                            <AddressIcon isActive={isActive("address")} />
                        </div>
                        <div className={`${isActive("address") ? "" : "text-gray-400"} flex flex-col items-center text-[12px] text-base/3 `}>
                            <span>Passo 1</span>
                            <span className='font-bold'>Endereço</span>
                        </div>
                    </div>

                    {/* Passo 2 - Envio */}
                    <div id="shipping" className="flex items-center gap-x-2">
                        <div>
                            <ShippingIcon isActive={isActive("shipping")} />
                        </div>
                        <div className={`${isActive("shipping") ? "" : "text-gray-400"} flex flex-col items-center text-[12px] text-base/3 `}>
                            <span>Passo 2</span>
                            <span className='font-bold'>Envio</span>
                        </div>
                    </div>

                    {/* Passo 3 - Pagamento */}
                    <div id="payment" className="flex items-center gap-x-2">
                        <div>
                            <PaymentIcon isActive={isActive("payment")} />
                        </div>
                        <div className={`${isActive("payment") ? "" : "text-gray-400"} flex flex-col items-center text-[12px] text-base/3 `}>
                            <span>Passo 3</span>
                            <span className='font-bold'>Pagamento</span>
                        </div>
                    </div>
                </div>

                <div className="my-20">
                    <div className='grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4 mt-4 '>
                        {
                            shippings.map(shipping => (
                                <ShippingCard
                                    key={shipping.id}
                                    data={shipping}
                                    checked={selectedShippingId === shipping.id}
                                    onChange={(id) => setSelectedShippingId(id)}
                                />
                            ))
                        }
                    </div>
                </div>

                <div className='flex items-center justify-end mt-10 gap-x-12'>
                    <button onClick={() => navigate('/cart/address/')} className='py-2 px-4 border border-text-primary hover:brightness-50 rounded cursor-pointer focus:outline-none transition transform active:scale-95 flex items-center justify-center'>
                        <span>Voltar </span>
                    </button>
                    <Button className="p-10" size="medium" onClick={() => navigate('/cart/address/shipping/payment')} >
                        <span className="text-white">Continuar</span>
                    </Button>
                </div>
            </Container>
        </section>
    );
};

export default Shipping;