/**
 * Interface para requisição de criação de usuário
 * Baseada na DTO CreateUserRequest do backend
 */
export interface CreateUserRequest {
  /** Primeiro nome do usuário (2-100 caracteres) */
  firstName: string;
  /** Último nome do usuário (2-155 caracteres) */
  lastName: string;
  /** Email do usuário (formato válido, máximo 255 caracteres) */
  email: string;
  /** Senha do usuário (mínimo 8 caracteres com maiúscula, minúscula, número e caractere especial) */
  password: string;
  /** Indica se o usuário optou por receber newsletter */
  newsletterOptIn: boolean;
}

/**
 * Interface para requisição de login de usuário
 * Baseada na DTO LoginUserRequest do backend
 */
export interface LoginUserRequest {
  /** Email do usuário (formato válido, máximo 255 caracteres) */
  email: string;
  /** Senha do usuário */
  password: string;
}
