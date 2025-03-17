import React from 'react';

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

interface State {
    formData: FormData;
    errors: Partial<Record<keyof FormData, string>>;
}
type Action =
    | { type: "UPDATE_FIELD"; field: keyof FormData; value: string }
    | { type: "SET_ERRORS"; errors: Partial<Record<keyof FormData, string>> };

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
}

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
const PlaceOrder: React.FC = () => {

    // o initialState vai para o state e o formReducer vai ser o dispatch
    const [state, dispatch] = React.useReducer(formReducer, initialState);
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
    }

    return (
        <div>
            Place Order
        </div>
    );
};

export default PlaceOrder;