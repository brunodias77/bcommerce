import React, { useContext, useState } from 'react';
import Container from '../components/Container';
import usePageName from '../hooks/UsePageName';
import { AddressIcon, ShippingIcon, PaymentIcon, EditIcon, AddIcon } from '../components/StepIcons';
import AddressCard from '../components/AddressCard';
import { Address as AddressType } from '../types/address-type';
import Button from '../components/Button';
import { ShopContext } from '../context/ShopContext';
import Divider from '../components/Divider';
import Modal from '../components/Modal';

const Address: React.FC = () => {
    const pageName = usePageName();
    const { navigate } = useContext(ShopContext);
    const isActive = (name: string) => pageName === name;
    const addresses: AddressType[] = [
        {
            id: 'addr1',
            street: "Rosa Iachel Mazetto, 195",
            city: "Marilia",
            state: "SP",
            zipCode: "01001-000",
            contactName: "João da Silva",
            phoneNumber: "(11) 91234-5678",
        },
        {
            id: 'addr2',
            street: "Rua da Liberdade, 321",
            city: "Rio de Janeiro",
            state: "RJ",
            zipCode: "20010-000",
            contactName: "Maria Oliveira",
            phoneNumber: "(21) 98888-1234",
        },
        {
            id: 'addr3',
            street: "Av. Brasil, 999",
            city: "Curitiba",
            state: "PR",
            zipCode: "80010-000",
            contactName: "Carlos Souza",
            phoneNumber: "(41) 99999-7777",
        },
    ];

    const [selected, setSelected] = useState<string>('addr1');
    const [isModalOpen, setIsModalOpen] = useState(false);

    return (
        <section className="bg-primary">
            <Container>
                <div id="steps" className="flex items-center justify-between gap-x-4 my-10">
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
                <div className='mt-20'>
                    <h3 className='text-primary font-bold text-xl'>Selecione o Endereço</h3>
                    <div className='grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4 mt-4 '>
                        {addresses.map(addr => (
                            <AddressCard
                                key={addr.id}
                                data={addr}
                                checked={selected === addr.id}
                                onChange={setSelected}
                            />
                        ))}
                    </div>
                </div>
                <Divider variant="button-icon" icon={AddIcon} label='adicionar um novo endereço' onClick={() => setIsModalOpen(true)}
                />
                {isModalOpen && (
                    <Modal isOpen={isModalOpen} onClose={() => setIsModalOpen(false)} />
                )}
                <div className='flex items-center justify-end mt-10 gap-x-12'>
                    <button onClick={() => navigate('/cart')} className='py-2 px-4 border border-text-primary hover:brightness-50 rounded cursor-pointer focus:outline-none transition transform active:scale-95 flex items-center justify-center'>
                        <span>Voltar </span>
                    </button>
                    <Button className="p-10" size="medium" onClick={() => navigate('/cart/address/shipping')} >
                        <span className="text-white">Continuar</span>
                    </Button>
                </div>
            </Container>
        </section >
    );
};

export default Address;


