import React, { useState, useContext } from 'react';
import { ShopContext } from '../context/ShopContext';

const Cupom: React.FC = () => {
    const [cupom, setCupom] = useState('');
    const { aplicarCupom } = useContext(ShopContext);

    const handleApply = () => {
        aplicarCupom(cupom.trim());
    };

    return (
        <div className="relative w-full my-6">
            <input
                type="text"
                value={cupom}
                onChange={(e) => setCupom(e.target.value)}
                placeholder="Digite seu cupom"
                className="w-full pr-24 p-3 border uppercase border-gray-200 rounded"
            />
            <button
                disabled={cupom.trim() === ''}
                onClick={handleApply}
                className={`cursor-pointer absolute top-1/2 right-2 -translate-y-1/2 px-4 py-1 rounded text-white transition ${cupom.trim() === ''
                    ? 'bg-gray-300 cursor-not-allowed'
                    : 'bg-yellow-primary hover:bg-yellow-primary-hover'
                    }`}
            >
                Aplicar
            </button>
        </div>
    );
};

export default Cupom;


// // components/Cupom.tsx
// import React, { useState } from 'react';
// import { toast } from 'react-toastify';
// import 'react-toastify/dist/ReactToastify.css';

// const Cupom: React.FC = () => {
//     const [cupom, setCupom] = useState('');

//     const handleApply = () => {
//         const isValid = validateCupom(cupom);
//         if (!isValid) {
//             toast.error('Cupom inválido!');
//         } else {
//             toast.success('Cupom aplicado com sucesso!');
//         }
//     };

//     const validateCupom = (code: string) => {
//         // Lógica mockada — substitua com sua verificação real
//         return code.toLowerCase() === 'desconto10';
//     };

//     return (
//         <div className="relative w-full my-6">
//             <input
//                 type="text"
//                 value={cupom}
//                 onChange={(e) => setCupom(e.target.value)}
//                 placeholder="Digite seu cupom"
//                 className="w-full pr-24 p-3 border uppercase border-gray-200 rounded"
//             />
//             <button
//                 disabled={cupom.trim() === ''}
//                 onClick={handleApply}
//                 className={`absolute top-1/2 right-2 -translate-y-1/2 px-4 py-1 rounded text-white transition ${cupom.trim() === ''
//                     ? 'bg-gray-300 cursor-not-allowed'
//                     : 'bg-yellow-400 hover:bg-yellow-500'
//                     }`}
//             >
//                 Aplicar
//             </button>
//         </div>
//     );
// };

// export default Cupom;
