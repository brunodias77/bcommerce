/**
 * Interface para resposta de criação de usuário
 * Baseada na DTO CreateUserResponse do backend
 */
export interface CreateUserResponse {
  /** ID único do usuário criado */
  userId: string;
  /** Email do usuário criado */
  email: string;
  /** Primeiro nome do usuário */
  firstName: string;
  /** Último nome do usuário */
  lastName: string;
  /** Nome completo do usuário */
  fullName?: string;
  /** Mensagem de sucesso */
  message: string;
  /** Data e hora da criação */
  createdAt: string;
  /** Indica se a operação foi bem-sucedida */
  success: boolean;
}
/**
 * Interface para erros de validação (resposta 400)
 */
export interface ValidationErrorResponse {
  errors: string[];
}

/**
 * Interface para erros gerais
 */
export interface ErrorResponse {
  message: string;
}