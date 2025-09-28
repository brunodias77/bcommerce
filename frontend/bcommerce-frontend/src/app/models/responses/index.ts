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

/**
 * Interface para resposta de login de usuário
 * Baseada na DTO LoginUserResponse do backend
 * Os campos seguem o padrão snake_case retornado pelo backend
 */
export interface LoginUserResponse {
  /** Token de acesso JWT */
  access_token: string;
  /** Tempo de expiração do token de acesso em segundos */
  expires_in: number;
  /** Tempo de expiração do refresh token em segundos */
  refresh_expires_in: number;
  /** Token de refresh para renovar o access token */
  refresh_token: string;
  /** Tipo do token (geralmente 'Bearer') */
  token_type: string;
  /** Escopo do token */
  scope?: string;
}

/**
 * Interface para resposta de ativação de conta
 * Baseada na resposta do endpoint /activate do backend
 */
export interface ActivateAccountResponse {
  /** Mensagem de sucesso ou erro */
  message: string;
}

/**
 * Interface para resposta de confirmação de email
 * Baseada na resposta do endpoint /activate do backend
 */
export interface ConfirmEmailResponse {
  /** Mensagem de sucesso ou erro */
  message: string;
}

/**
 * Interface para resposta de redefinição de senha
 * Baseada na DTO ResetPasswordResponse do backend
 */
export interface ResetPasswordResponse {
  /** Indica se a operação foi bem-sucedida */
  success: boolean;
  /** Mensagem de sucesso ou erro */
  message: string;
}