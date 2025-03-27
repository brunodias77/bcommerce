import React from 'react';
import Container from '../components/Container';
import AddressIcon from '../assets/icons/address.svg';

const Address: React.FC = () => {
    return (
        <section className="bg-primary">
            <Container >
                <div id='steps' className='flex items-center justify-between gap-x-4'>
                    <div className='flex items-center gap-x-2'>
                        <div>
                            <img src={AddressIcon} alt="icone de um telefone" />
                        </div>
                        <div className='flex flex-col items-center text-[12px] text-base/3 font-bold'>
                            <span>Passo 1</span>
                            <span>Endereco</span>
                        </div>
                    </div>

                    <div className='flex items-center gap-x-2'>
                        <div>
                            <img src={AddressIcon} alt="icone de um telefone" />
                        </div>
                        <div className='flex flex-col items-center text-[12px] text-base/3 font-bold'>
                            <span>Passo 1</span>
                            <span>Endereco</span>
                        </div>
                    </div>

                    <div className='flex items-center gap-x-2'>
                        <div>
                            <img src={AddressIcon} alt="icone de um telefone" />
                        </div>
                        <div className='flex flex-col items-center text-[12px] text-base/3 font-bold'>
                            <span>Passo 1</span>
                            <span>Endereco</span>
                        </div>
                    </div>
                </div>
            </Container>
        </section>
    );
}

export default Address;