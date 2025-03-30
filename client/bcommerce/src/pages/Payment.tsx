import CreditCard from "../components/CreditCardForms";
import Container from "../components/Container";
import { AddressIcon, PaymentIcon, ShippingIcon } from "../components/StepIcons";
import usePageName from '../hooks/UsePageName';

const Payment: React.FC = () => {
    const pageName = usePageName();
    const isActive = (name: string) => pageName === name;

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
                <CreditCard />
            </Container>
        </section>
    );
}

export default Payment;