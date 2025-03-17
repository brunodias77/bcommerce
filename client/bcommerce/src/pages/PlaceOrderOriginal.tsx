import { useReducer, useState, ChangeEvent, FormEvent } from "react";
import CartTotal from "../components/CartTotal";
import Title from "../components/Title";
import Footer from "../components/Footer";
import Toast from "../context/Toast";

/** 
 * @summary Define a estrutura dos dados do formulário
 */
interface FormData {
    firstName: string;
    lastName: string;
    email: string;
    phone: string;
    street: string;
    city: string;
    zip: string;
    country: string;
}

/** 
 * @summary Define o estado do formulário, incluindo os dados e possíveis erros
 */
interface State {
    formData: FormData;
    errors: Partial<Record<keyof FormData, string>>;
}

/** 
 * @summary Define os tipos de ações que podem ser despachadas para o reducer
 */
type Action =
    | { type: "UPDATE_FIELD"; field: keyof FormData; value: string }
    | { type: "SET_ERRORS"; errors: Partial<Record<keyof FormData, string>> };

/** 
 * @summary Estado inicial do formulário
 */
const initialState: State = {
    formData: {
        firstName: "",
        lastName: "",
        email: "",
        phone: "",
        street: "",
        city: "",
        zip: "",
        country: "",
    },
    errors: {},
};

/** 
 * @summary Reducer responsável por gerenciar o estado do formulário
 * @param {State} state - Estado atual
 * @param {Action} action - Ação que define como o estado será atualizado
 * @returns {State} Novo estado atualizado
 */
function formReducer(state: State, action: Action): State {
    switch (action.type) {
        case "UPDATE_FIELD":
            return {
                ...state,
                formData: {
                    ...state.formData,
                    [action.field]: action.value,
                },
                errors: {
                    ...state.errors,
                    [action.field]: "", // Limpa erro ao digitar
                },
            };
        case "SET_ERRORS":
            return { ...state, errors: action.errors };
        default:
            return state;
    }
}

/** 
 * @summary Componente principal responsável pela página de finalização do pedido
 */
const PlaceOrder: React.FC = () => {
    /** @summary Estado do formulário gerenciado pelo reducer */
    const [state, dispatch] = useReducer(formReducer, initialState);

    /** @summary Estado do método de pagamento selecionado */
    const [method, setMethod] = useState<"stripe" | "cod">("cod");

    /** @summary Estado para exibição do Toast */
    const [showToast, setShowToast] = useState<boolean>(false);

    /** 
     * @summary Função para validar os campos do formulário
     * @returns {boolean} Retorna true se o formulário for válido, false caso contrário
     */
    const validateForm = (): boolean => {
        const newErrors: Partial<Record<keyof FormData, string>> = {};
        const { formData } = state;

        const validations: Record<keyof FormData, { test: (v: string) => boolean; message: string }> = {
            firstName: {
                test: (v) => v.trim() !== "",
                message: "Por favor, insira seu nome e sobrenome",
            },
            lastName: {
                test: (v) => v.trim() !== "",
                message: "Por favor, insira seu sobrenome",
            },
            email: {
                test: (v) => /\S+@\S+\.\S+/.test(v),
                message: "E-mail inválido",
            },
            phone: {
                test: (v) => /^0\d{9}$/.test(v),
                message: "Número de telefone inválido",
            },
            street: {
                test: (v) => v.trim() !== "",
                message: "Por favor, insira o nome da rua",
            },
            city: {
                test: (v) => v.trim() !== "",
                message: "Por favor, insira a cidade",
            },
            zip: {
                test: (v) => /^\d{5,6}$/.test(v),
                message: "Código postal inválido",
            },
            country: {
                test: (v) => v.trim() !== "",
                message: "Por favor, insira o país",
            },
        };

        Object.entries(validations).forEach(([field, rule]) => {
            const key = field as keyof FormData;
            if (!rule.test(formData[key])) {
                newErrors[key] = rule.message;
            }
        });

        dispatch({ type: "SET_ERRORS", errors: newErrors });

        return Object.keys(newErrors).length === 0;
    };

    /** 
     * @summary Manipula mudanças nos inputs e atualiza o estado do formulário
     * @param {ChangeEvent<HTMLInputElement>} e - Evento de mudança no input
     */
    const handleChange = (e: ChangeEvent<HTMLInputElement>) => {
        dispatch({ type: "UPDATE_FIELD", field: e.target.name as keyof FormData, value: e.target.value });
    };

    /** 
     * @summary Manipula o envio do formulário
     * @param {FormEvent} e - Evento de envio do formulário
     */
    const handleSubmit = (e: FormEvent) => {
        e.preventDefault();
        if (validateForm()) {
            setShowToast(true);
        }
    };

    return (
        <div>
            <div className="bg-primary mb-16">
                <form className="max-padd-container py-10" onSubmit={handleSubmit}>
                    <div className="flex flex-col xl:flex-row gap-20 xl:gap-28">
                        {/* Lado Esquerdo */}
                        <div className="flex-1 flex flex-col gap-3 text-[95%]">
                            <Title title1="Informações" title2="de Entrega" />

                            {/* Campos de Entrada Dinâmicos */}
                            {[
                                { name: "firstName", label: "Nome" },
                                { name: "lastName", label: "Sobrenome" },
                                { name: "email", label: "E-mail" },
                                { name: "phone", label: "Telefone" },
                                { name: "street", label: "Rua" },
                                { name: "city", label: "Cidade" },
                                { name: "zip", label: "Código Postal" },
                                { name: "country", label: "País" },
                            ].map(({ name, label }) => (
                                <div key={name}>
                                    <input
                                        type="text"
                                        name={name}
                                        placeholder={`${label}...`}
                                        className="ring-1 ring-slate-900/15 p-1 pl-3 rounded-sm bg-white outline-none w-full"
                                        value={state.formData[name as keyof FormData]}
                                        onChange={handleChange}
                                    />
                                    {state.errors[name as keyof FormData] && (
                                        <p className="text-red-500 text-sm">{state.errors[name as keyof FormData]}</p>
                                    )}
                                </div>
                            ))}
                        </div>

                        {/* Lado Direito */}
                        <div className="flex flex-col flex-1">
                            <CartTotal />
                            <div className="my-6">
                                <h3 className="bold-20 mb-5">
                                    Método de <span>Pagamento</span>
                                </h3>
                                <div className="flex gap-3">
                                    {["stripe", "cod"].map((payment) => (
                                        <div
                                            key={payment}
                                            onClick={() => setMethod(payment as "stripe" | "cod")}
                                            className={`${method === payment ? "btn-dark" : "btn-white"} !py-1 text-xs cursor-pointer`}
                                        >
                                            {payment === "stripe" ? "Cartão (Stripe)" : "Dinheiro na Entrega"}
                                        </div>
                                    ))}
                                </div>
                            </div>
                            <button type="submit" className="btn-secondary" disabled={Object.keys(state.errors).length > 0}>
                                Finalizar Pedido
                            </button>
                            {showToast && <Toast message="Obrigado por comprar conosco!" onClose={() => setShowToast(false)} />}
                        </div>
                    </div>
                </form>
            </div>
            <Footer />
        </div>
    );
};

export default PlaceOrder;
