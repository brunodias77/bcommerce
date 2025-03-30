import React from 'react';
import Input from './Input';

interface ModalProps {
    isOpen: boolean;
    onClose: () => void;
}

const Modal: React.FC<ModalProps> = ({ isOpen, onClose }) => {
    if (!isOpen) return null;

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-50">
            <div className="bg-white rounded-lg shadow w-full max-w-md mx-4 md:mx-0 p-6 relative">
                {/* Botão fechar */}
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

                <form className="space-y-6">
                    <h3 className="text-xl font-medium text-gray-900">
                        Sign in to our platform
                    </h3>

                    <Input
                        id="email"
                        label="Your email"
                        type="email"
                        placeholder="name@company.com"
                        required
                    />

                    <Input
                        id="password"
                        label="Your password"
                        type="password"
                        placeholder="••••••••"
                        required
                    />

                    <div className="flex justify-between items-center">
                        <label className="flex items-center text-sm text-gray-700">
                            <input
                                type="checkbox"
                                className="mr-2 w-4 h-4 text-blue-600 bg-white border-gray-300 rounded"
                            />
                            Remember me
                        </label>
                        <a href="#" className="text-sm text-blue-700 hover:underline">
                            Lost Password?
                        </a>
                    </div>

                    <button
                        type="submit"
                        className="w-full bg-blue-700 text-white hover:bg-blue-800 focus:ring-4 focus:ring-blue-300 font-medium rounded-lg text-sm px-5 py-2.5"
                    >
                        Login to your account
                    </button>

                    <p className="text-sm text-gray-500 text-center">
                        Not registered?{' '}
                        <a href="#" className="text-blue-700 hover:underline">
                            Create account
                        </a>
                    </p>
                </form>
            </div>
        </div>
    );
};

export default Modal;


// import React from 'react';

// interface ModalProps {
//     isOpen: boolean;
//     onClose: () => void;
// }

// const Modal: React.FC<ModalProps> = ({ isOpen, onClose }) => {
//     if (!isOpen) return null;

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

//                 <form className="space-y-6">
//                     <h3 className="text-xl font-medium text-gray-900">
//                         Sign in to our platform
//                     </h3>

//                     <div>
//                         <label htmlFor="email" className="block text-sm font-medium text-gray-700 mb-2">
//                             Your email
//                         </label>
//                         <input
//                             type="email"
//                             id="email"
//                             className="w-full p-2.5 text-sm text-gray-900 border border-gray-300 rounded-lg bg-white focus:ring-blue-500 focus:border-blue-500"
//                             placeholder="name@company.com"
//                             required
//                         />
//                     </div>

//                     <div>
//                         <label htmlFor="password" className="block text-sm font-medium text-gray-700 mb-2">
//                             Your password
//                         </label>
//                         <input
//                             type="password"
//                             id="password"
//                             className="w-full p-2.5 text-sm text-gray-900 border border-gray-300 rounded-lg bg-white focus:ring-blue-500 focus:border-blue-500"
//                             placeholder="••••••••"
//                             required
//                         />
//                     </div>

//                     <div className="flex justify-between items-center">
//                         <label className="flex items-center text-sm text-gray-700">
//                             <input
//                                 type="checkbox"
//                                 className="mr-2 w-4 h-4 text-blue-600 bg-white border-gray-300 rounded"
//                             />
//                             Remember me
//                         </label>
//                         <a href="#" className="text-sm text-blue-700 hover:underline">
//                             Lost Password?
//                         </a>
//                     </div>

//                     <button
//                         type="submit"
//                         className="w-full bg-blue-700 text-white hover:bg-blue-800 focus:ring-4 focus:ring-blue-300 font-medium rounded-lg text-sm px-5 py-2.5"
//                     >
//                         Login to your account
//                     </button>

//                     <p className="text-sm text-gray-500 text-center">
//                         Not registered?{' '}
//                         <a href="#" className="text-blue-700 hover:underline">
//                             Create account
//                         </a>
//                     </p>
//                 </form>
//             </div>
//         </div>
//     );
// };

// export default Modal;
