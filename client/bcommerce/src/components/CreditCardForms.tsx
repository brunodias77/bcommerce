import React, { useReducer, useState, ChangeEvent, useContext } from 'react';
import Input from './Input';
import CardFront from '../assets/others/cardFront.png';
import CardBack from '../assets/others/cardBack.png';
import Button from './Button';
import { ShopContext } from '../context/ShopContext';

type ExpiryDate = {
    month: string;
    year: string;
};

type CardSide = 'front' | 'back';

type CardState = {
    cardholder: string;
    cardNumber: string;
    expired: ExpiryDate;
    securityCode: string;
};

type Action =
    | { type: 'SET_CARDHOLDER'; payload: string }
    | { type: 'SET_CARD_NUMBER'; payload: string }
    | { type: 'SET_EXPIRED'; payload: { month?: string; year?: string } }
    | { type: 'SET_SECURITY_CODE'; payload: string };

const initialState: CardState = {
    cardholder: '',
    cardNumber: '',
    expired: { month: '', year: '' },
    securityCode: '',
};

function reducer(state: CardState, action: Action): CardState {
    switch (action.type) {
        case 'SET_CARDHOLDER':
            return { ...state, cardholder: action.payload };
        case 'SET_CARD_NUMBER':
            return { ...state, cardNumber: action.payload };
        case 'SET_EXPIRED':
            return { ...state, expired: { ...state.expired, ...action.payload } };
        case 'SET_SECURITY_CODE':
            return { ...state, securityCode: action.payload };
        default:
            return state;
    }
}

export default function CreditCardForm() {
    const [state, dispatch] = useReducer(reducer, initialState);
    const [cardSide, setCardSide] = useState<CardSide>('front');
    const { createOrder } = useContext(ShopContext);

    const formatCardNumber = (value: string): string =>
        value.replace(/\W/gi, '').replace(/(.{4})/g, '$1 ').trim();

    const isValid = (): boolean =>
        state.cardholder.length >= 5 &&
        state.cardNumber.length >= 19 &&
        state.expired.month !== '' &&
        state.expired.year !== '' &&
        state.securityCode.length === 3;

    const handleSubmit = (): void => {
        alert(`You did it ${state.cardholder}.`);
    };

    const handleInputChange = (
        e: ChangeEvent<HTMLInputElement | HTMLSelectElement>
    ) => {
        const { name, value } = e.target;
        if (name === 'month' || name === 'year') {
            dispatch({ type: 'SET_EXPIRED', payload: { [name]: value } });
        }
    };

    return (
        <div className="m-4">
            <div className="credit-card w-full sm:w-auto shadow-lg mx-auto rounded-xl bg-white">
                <header className="flex flex-col justify-center items-center">
                    {cardSide === 'front' ? (
                        <div className="relative transition-all duration-300">
                            <img
                                className="w-full h-auto"
                                src={CardFront}
                                alt="front credit card"
                            />
                            <div className="front bg-transparent text-lg w-full text-white px-12 absolute left-0 bottom-12">
                                <p className="number mb-5 sm:text-xl">
                                    {state.cardNumber || '0000 0000 0000 0000'}
                                </p>
                                <div className="flex flex-row justify-between">
                                    <p>{state.cardholder || 'Card holder'}</p>
                                    <div>
                                        {state.expired.month && <span>{state.expired.month}/</span>}
                                        <span>{state.expired.year}</span>
                                    </div>
                                </div>
                            </div>
                        </div>
                    ) : (
                        <div className="relative transition-all duration-300">
                            <img
                                className="w-full h-auto"
                                src={CardBack}
                                alt="back credit card"
                            />
                            <div className="bg-transparent text-white text-xl w-full flex justify-end absolute bottom-20 px-8 sm:bottom-24 right-0 sm:px-12">
                                <div className="border border-white w-16 h-9 flex justify-center items-center">
                                    <p>{state.securityCode || 'code'}</p>
                                </div>
                            </div>
                        </div>
                    )}

                    <ul className="flex mt-4">
                        <li className="mx-2">
                            <img className="w-16" src="https://www.computop-paygate.com/Templates/imagesaboutYou_desktop/images/computop.png" alt="" />
                        </li>
                        <li className="mx-2">
                            <img className="w-14" src="https://www.computop-paygate.com/Templates/imagesaboutYou_desktop/images/verified-by-visa.png" alt="" />
                        </li>
                        <li className="ml-5">
                            <img className="w-7" src="https://www.computop-paygate.com/Templates/imagesaboutYou_desktop/images/mastercard-id-check.png" alt="" />
                        </li>
                    </ul>
                </header>

                <main className="mt-4 p-4">
                    <h1 className="text-xl font-semibold text-gray-700 text-center">Pagamento com cartao de credito</h1>

                    <div className="my-3">
                        <Input
                            id="cardholder"
                            label="Nome do titular"
                            type="text"
                            maxLength={22}
                            value={state.cardholder}
                            onChange={(e) => dispatch({ type: 'SET_CARDHOLDER', payload: e.target.value })}
                            placeholder="Card holder"
                        />
                    </div>
                    <div className="my-3">
                        <Input
                            id="cardNumber"
                            label="Numero do cartao"
                            type="text"
                            maxLength={19}
                            value={state.cardNumber}
                            onChange={(e) =>
                                dispatch({ type: 'SET_CARD_NUMBER', payload: formatCardNumber(e.target.value) })
                            }
                            placeholder="Card number"
                        />
                    </div>

                    <div className="my-3">
                        <label className="text-gray-700">Vencimento</label>
                        <div className="grid grid-cols-2 sm:grid-cols-4 gap-2 mt-2">
                            <select
                                className="form-select"
                                name="month"
                                value={state.expired.month}
                                onChange={handleInputChange}
                            >
                                <option value="" disabled>MM</option>
                                {Array.from({ length: 12 }, (_, i) => {
                                    const month = `${i + 1}`.padStart(2, '0');
                                    return <option key={month} value={month}>{month}</option>;
                                })}
                            </select>

                            <select
                                className="form-select"
                                name="year"
                                value={state.expired.year}
                                onChange={handleInputChange}
                            >
                                <option value="" disabled>YY</option>
                                {[2024, 2025, 2026].map((year) => (
                                    <option key={year} value={year.toString()}>{year}</option>
                                ))}
                            </select>

                            <div className="col-span-2">
                                <Input
                                    id="securityCode"
                                    label="Codigo de seguranca"
                                    type="text"
                                    maxLength={3}
                                    value={state.securityCode}
                                    onFocus={() => setCardSide('back')}
                                    onBlur={() => setCardSide('front')}
                                    onChange={(e) =>
                                        dispatch({ type: 'SET_SECURITY_CODE', payload: e.target.value })
                                    }
                                    placeholder="CVV"
                                />
                            </div>
                        </div>
                    </div>
                </main>

                <footer className="mt-6 p-4">
                    <Button
                        variant="primary"
                        size="large"
                        fullWidth
                        disabled={!isValid()}
                        onClick={createOrder}
                    >
                        Finalizar compra
                    </Button>
                </footer>
            </div>
        </div>
    );
}

