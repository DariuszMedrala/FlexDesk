import type { AuthResponse } from '../types/models'
import { apiFetch } from './client'

export function login(email: string, password: string): Promise<AuthResponse> {
  return apiFetch<AuthResponse>('/auth/login', {
    method: 'POST',
    body: JSON.stringify({ email, password }),
  })
}

export function register(
  email: string,
  password: string,
  firstName: string,
  lastName: string,
): Promise<AuthResponse> {
  return apiFetch<AuthResponse>('/auth/register', {
    method: 'POST',
    body: JSON.stringify({ email, password, firstName, lastName }),
  })
}
