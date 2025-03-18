import React, { ChangeEvent, FormEvent, useReducer, useState } from "react";
import Toast from "../context/Toast";
import CartTotal from "../components/CartTotal";
import Title from "../components/Title";

/** Interface para os dados do formulário */
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

/** Interface para o estado do formulário */
interface State {
    formData: FormData;
    errors: Partial<Record<keyof FormData, string>>;
}

/** Definição das ações do reducer */
type Action =
    | { type: "UPDATE_FIELD"; field: keyof FormData; value: string }
    | { type: "SET_ERRORS"; errors: Partial<Record<keyof FormData, string>> };

/** Estado inicial do formulário */
const initialState: State = {
    formData: { firstName: "", lastName: "", email: "", phone: "", street: "", city: "", zip: "", country: "" },
    errors: {},
};

/** Reducer para manipular o estado do formulário */
const formReducer = (state: State, action: Action): State => {
    switch (action.type) {
        case "UPDATE_FIELD":
            return {
                ...state,
                formData: { ...state.formData, [action.field]: action.value },
                errors: { ...state.errors, [action.field]: "" }
            };
        case "SET_ERRORS":
            return { ...state, errors: action.errors };
        default:
            return state;
    }
};

/** Regras de validação dos campos */
const validations: Record<keyof FormData, { test: (v: string) => boolean; message: string }> = {
    firstName: { test: (v) => v.trim() !== "", message: "Por favor, insira seu nome" },
    lastName: { test: (v) => v.trim() !== "", message: "Por favor, insira seu sobrenome" },
    email: { test: (v) => /\S+@\S+\.\S+/.test(v), message: "E-mail inválido" },
    phone: { test: (v) => /^0\d{9}$/.test(v), message: "Número de telefone inválido" },
    street: { test: (v) => v.trim() !== "", message: "Por favor, insira o nome da rua" },
    city: { test: (v) => v.trim() !== "", message: "Por favor, insira a cidade" },
    zip: { test: (v) => /^\d{5,6}$/.test(v), message: "Código postal inválido" },
    country: { test: (v) => v.trim() !== "", message: "Por favor, insira o país" },
};

/** Componente reutilizável para os inputs */
const TextInput: React.FC<{ name: keyof FormData; label: string; value: string; onChange: (e: ChangeEvent<HTMLInputElement>) => void; error?: string }> = ({ name, label, value, onChange, error }) => (
    <div>
        <input
            type="text"
            name={name}
            placeholder={`${label}...`}
            className="ring-1 ring-slate-900/15 p-1 pl-3 rounded-sm bg-white outline-none w-full"
            value={value}
            onChange={onChange}
        />
        {error && <p className="text-red-500 text-sm">{error}</p>}
    </div>
);

/** Lista dos métodos de pagamento */
const paymentMethods: { id: "stripe" | "cod"; label: string }[] = [
    { id: "stripe", label: "Cartão (Stripe)" },
    { id: "cod", label: "Dinheiro na Entrega" },
];

const PlaceOrder: React.FC = () => {
    const [state, dispatch] = useReducer(formReducer, initialState);
    const [paymentMethod, setPaymentMethod] = useState<"stripe" | "cod">("cod");
    const [showToast, setShowToast] = useState(false);

    /** Função para validar os campos do formulário */
    const validateForm = (): boolean => {
        const { formData } = state;
        const newErrors: Partial<Record<keyof FormData, string>> = {};

        Object.entries(validations).forEach(([field, rule]) => {
            const key = field as keyof FormData;
            if (!rule.test(formData[key])) {
                newErrors[key] = rule.message;
            }
        });

        dispatch({ type: "SET_ERRORS", errors: newErrors });

        return Object.keys(newErrors).length === 0;
    };

    /** Manipula a mudança nos inputs */
    const handleChange = (e: ChangeEvent<HTMLInputElement>) => {
        dispatch({ type: "UPDATE_FIELD", field: e.target.name as keyof FormData, value: e.target.value });
    };

    /** Manipula o envio do formulário */
    const handleSubmit = (e: FormEvent) => {
        e.preventDefault();
        if (validateForm()) setShowToast(true);
        console.log("Mostrando o state", state);
    };

    return (
        <div>
            <div className="bg-primary mb-16">
                <form className="max-padd-container py-10" onSubmit={handleSubmit}>
                    <div className="flex flex-col xl:flex-row gap-20 xl:gap-28">
                        {/* LADO ESQUERDO */}
                        <div className="flex-1 flex flex-col gap-3 text-[95%]">
                            <Title title="Delivery" subtitle=" Information" />
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
                                <TextInput
                                    key={name}
                                    name={name as keyof FormData}
                                    label={label}
                                    value={state.formData[name as keyof FormData]}
                                    onChange={handleChange}
                                    error={state.errors[name as keyof FormData]}
                                />
                            ))}
                        </div>

                        {/* LADO DIREITO */}
                        <div className="flex flex-col flex-1">
                            <CartTotal />
                            <div className="my-6">
                                <h3 className="bold-20 mb-5">
                                    Metodo de <span>Pagamento</span>
                                </h3>
                                <div className="flex gap-3">
                                    {["stripe", "cod"].map((payment) => (
                                        <div
                                            key={payment}
                                            onClick={() => setPaymentMethod(payment as "stripe" | "cod")}
                                            className={`${paymentMethod === payment ? "btn-dark" : "btn-white"} !py-1 text-xs cursor-pointer`}
                                        >
                                            {payment === "stripe" ? "Cartão (Stripe)" : "Dinheiro na Entrega"}
                                        </div>
                                    ))}
                                </div>
                            </div>
                            {/* Botão de Envio */}
                            <button type="submit" className="btn-secondary">
                                Finalizar Pedido
                            </button>
                            {showToast && <Toast message="Obrigado por comprar conosco!" onClose={() => setShowToast(false)} />}
                        </div>
                    </div>
                </form>
            </div>
        </div>
    );
};

export default PlaceOrder;

