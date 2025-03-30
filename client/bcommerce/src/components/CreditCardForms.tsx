import React, { useState, ChangeEvent } from 'react';

type ExpiryDate = {
    month: string;
    year: string;
};

type CardSide = 'front' | 'back';

export default function CreditCardForm() {
    const [cardholder, setCardholder] = useState<string>('');
    const [cardNumber, setCardNumber] = useState<string>('');
    const [expired, setExpired] = useState<ExpiryDate>({ month: '', year: '' });
    const [securityCode, setSecurityCode] = useState<string>('');
    const [cardSide, setCardSide] = useState<CardSide>('front');

    const formatCardNumber = (value: string): string => {
        return value.replace(/\W/gi, '').replace(/(.{4})/g, '$1 ').trim();
    };

    const isValid = (): boolean => {
        return (
            cardholder.length >= 5 &&
            cardNumber.length >= 19 &&
            expired.month !== '' &&
            expired.year !== '' &&
            securityCode.length === 3
        );
    };

    const handleSubmit = (): void => {
        alert(`You did it ${cardholder}.`);
    };

    const handleInputChange = (
        e: ChangeEvent<HTMLInputElement | HTMLSelectElement>
    ) => {
        const { name, value } = e.target;
        if (name === 'month' || name === 'year') {
            setExpired((prev) => ({ ...prev, [name]: value }));
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
                                src="https://www.computop-paygate.com/Templates/imagesaboutYou_desktop/images/svg-cards/card-visa-front.png"
                                alt="front credit card"
                            />
                            <div className="front bg-transparent text-lg w-full text-white px-12 absolute left-0 bottom-12">
                                <p className="number mb-5 sm:text-xl">
                                    {cardNumber || '0000 0000 0000 0000'}
                                </p>
                                <div className="flex flex-row justify-between">
                                    <p>{cardholder || 'Card holder'}</p>
                                    <div>
                                        {expired.month && <span>{expired.month}/</span>}
                                        <span>{expired.year}</span>
                                    </div>
                                </div>
                            </div>
                        </div>
                    ) : (
                        <div className="relative transition-all duration-300">
                            <img
                                className="w-full h-auto"
                                src="https://www.computop-paygate.com/Templates/imagesaboutYou_desktop/images/svg-cards/card-visa-back.png"
                                alt="back credit card"
                            />
                            <div className="bg-transparent text-white text-xl w-full flex justify-end absolute bottom-20 px-8 sm:bottom-24 right-0 sm:px-12">
                                <div className="border border-white w-16 h-9 flex justify-center items-center">
                                    <p>{securityCode || 'code'}</p>
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
                    <h1 className="text-xl font-semibold text-gray-700 text-center">Card payment</h1>

                    <div className="my-3">
                        <input
                            type="text"
                            className="input-field"
                            placeholder="Card holder"
                            maxLength={22}
                            value={cardholder}
                            onChange={(e) => setCardholder(e.target.value)}
                        />
                    </div>
                    <div className="my-3">
                        <input
                            type="text"
                            className="input-field"
                            placeholder="Card number"
                            maxLength={19}
                            value={cardNumber}
                            onChange={(e) => setCardNumber(formatCardNumber(e.target.value))}
                        />
                    </div>

                    <div className="my-3">
                        <label className="text-gray-700">Expired</label>
                        <div className="grid grid-cols-2 sm:grid-cols-4 gap-2 mt-2">
                            <select
                                className="form-select"
                                name="month"
                                value={expired.month}
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
                                value={expired.year}
                                onChange={handleInputChange}
                            >
                                <option value="" disabled>YY</option>
                                {[2024, 2025, 2026].map((year) => (
                                    <option key={year} value={year.toString()}>{year}</option>
                                ))}
                            </select>

                            <input
                                type="text"
                                className="input-field col-span-2"
                                placeholder="Security code"
                                maxLength={3}
                                value={securityCode}
                                onFocus={() => setCardSide('back')}
                                onBlur={() => setCardSide('front')}
                                onChange={(e) => setSecurityCode(e.target.value)}
                            />
                        </div>
                    </div>
                </main>

                <footer className="mt-6 p-4">
                    <button
                        className="submit-button"
                        disabled={!isValid()}
                        onClick={handleSubmit}
                    >
                        Pay now
                    </button>
                </footer>
            </div>
        </div>
    );
}
