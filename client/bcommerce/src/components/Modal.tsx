import React, { useRef, useContext } from 'react';
import Input from './Input';
import Button from './Button';
import { toast } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';
import { ShopContext } from '../context/ShopContext';

interface ModalProps {
    isOpen: boolean;
    onClose: () => void;
}

const Modal: React.FC<ModalProps> = ({ isOpen, onClose }) => {
    const formRef = useRef<HTMLFormElement>(null);
    const { navigate } = useContext(ShopContext);

    if (!isOpen) return null;

    const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        if (!formRef.current) return;

        const formData = new FormData(formRef.current);
        const data = Object.fromEntries(formData.entries());

        const requiredFields = ['cep', 'logradouro', 'numero', 'bairro', 'cidade', 'estado'];
        const missing = requiredFields.filter((field) => !data[field]);

        if (missing.length > 0) {
            toast.error('Preencha todos os campos obrigatórios! ❌');
            return;
        }

        console.log('Endereço cadastrado:', data);
        toast.success('Endereço salvo com sucesso! 📦');

        // Fecha o modal após um pequeno delay
        setTimeout(() => {
            onClose();
        }, 1500);
    };

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-50">
            <div className="bg-white rounded-lg shadow w-full max-w-md mx-4 md:mx-0 p-6 relative">
                <button
                    onClick={onClose}
                    className="absolute top-2 right-2 text-gray-400 hover:text-gray-900"
                >
                    <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
                        <path
                            fillRule="evenodd"
                            clipRule="evenodd"
                            d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 
              4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 
              01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z"
                        />
                    </svg>
                </button>

                <form ref={formRef} onSubmit={handleSubmit} className="space-y-4">
                    <h3 className="text-xl font-medium text-gray-900">Novo Endereço</h3>

                    <Input id="cep" name="cep" label="CEP" placeholder="Digite o CEP" required />
                    <Input id="logradouro" name="logradouro" label="Logradouro" placeholder="Digite o logradouro" required />
                    <Input id="numero" name="numero" label="Número" placeholder="Digite o número" required />
                    <Input id="bairro" name="bairro" label="Bairro" placeholder="Digite o bairro" required />
                    <Input id="cidade" name="cidade" label="Cidade" placeholder="Digite a cidade" required />
                    <Input id="estado" name="estado" label="Estado" placeholder="Digite o estado" required />

                    <Button type="submit" variant="primary" size="medium" fullWidth label="Salvar Endereço">
                        Salvar Endereço
                    </Button>
                </form>
            </div>
        </div>
    );
};

export default Modal;



// import React, { useRef } from 'react';
// import Input from './Input';

// interface ModalProps {
//     isOpen: boolean;
//     onClose: () => void;
// }

// const Modal: React.FC<ModalProps> = ({ isOpen, onClose }) => {
//     const formRef = useRef<HTMLFormElement>(null);

//     if (!isOpen) return null;

//     const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
//         e.preventDefault();
//         if (!formRef.current) return;

//         const formData = new FormData(formRef.current);
//         console.log(Object.fromEntries(formData.entries()));
//     };

//     return (
//         <div className="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-50">
//             <div className="bg-white rounded-lg shadow w-full max-w-md mx-4 md:mx-0 p-6 relative">
//                 {/* Botão fechar */}
//                 <button
//                     onClick={onClose}
//                     className="absolute top-2 right-2 text-gray-400 hover:text-gray-900"
//                 >
//                     <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
//                         <path
//                             fillRule="evenodd"
//                             clipRule="evenodd"
//                             d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 
//               4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 
//               01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z"
//                         />
//                     </svg>
//                 </button>

//                 <form ref={formRef} onSubmit={handleSubmit} className="space-y-4">
//                     <h3 className="text-xl font-medium text-gray-900">Contato</h3>

//                     <Input
//                         id="nome"
//                         label="Nome"
//                         placeholder="Digite seu nome"
//                         required
//                     />
//                     <input type="hidden" name="nome" id="nome" />

//                     <Input
//                         id="email"
//                         label="Email"
//                         type="email"
//                         placeholder="seu@email.com"
//                         required
//                     />
//                     <input type="hidden" name="email" id="email" />

//                     <div>
//                         <label htmlFor="mensagem" className="block text-sm font-medium text-gray-700 mb-2">
//                             Mensagem
//                         </label>
//                         <textarea
//                             id="mensagem"
//                             name="mensagem"
//                             placeholder="Escreva sua mensagem"
//                             required
//                             className="w-full p-2.5 text-sm text-gray-900 border border-gray-300 rounded-lg bg-white focus:ring-blue-500 focus:border-blue-500"
//                         />
//                     </div>

//                     <button
//                         type="submit"
//                         className="w-full bg-blue-700 text-white hover:bg-blue-800 focus:ring-4 focus:ring-blue-300 font-medium rounded-lg text-sm px-5 py-2.5"
//                     >
//                         Enviar
//                     </button>
//                 </form>
//             </div>
//         </div>
//     );
// };

// export default Modal;


// import React, { useRef } from 'react';
// import Input from './Input';

// interface ModalProps {
//     isOpen: boolean;
//     onClose: () => void;
// }

// const Modal: React.FC<ModalProps> = ({ isOpen, onClose }) => {
//     const formRef = useRef<HTMLFormElement>(null);

//     if (!isOpen) return null;

//     const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
//         e.preventDefault();
//         if (!formRef.current) return;
//         const formData = new FormData(formRef.current);
//         console.log(Object.fromEntries(formData.entries()));
//     };


//     return (
//         <div className="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-50">
//             <div className="bg-white rounded-lg shadow w-full max-w-md mx-4 md:mx-0 p-6 relative">
//                 {/* Botão fechar */}
//                 <button
//                     onClick={onClose}
//                     className="absolute top-2 right-2 text-gray-400 hover:text-gray-900"
//                 >
//                     <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
//                         <path
//                             fillRule="evenodd"
//                             clipRule="evenodd"
//                             d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 
//               4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 
//               01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z"
//                         />
//                     </svg>
//                 </button>
//                 <form ref={formRef} onSubmit={handleSubmit}>
//                     <input name="nome" placeholder="Nome" />
//                     <input name="email" type="email" placeholder="Email" />
//                     <textarea name="mensagem" placeholder="Mensagem" />
//                     <button type="submit">Enviar</button>
//                 </form>
//             </div>
//         </div>
//     );
// };

// export default Modal;



