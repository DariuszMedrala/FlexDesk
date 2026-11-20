import type { UserRole } from '../types/models'
import { apiFetch } from './client'

export interface Profile {
  id: string
  email: string
  firstName: string
  lastName: string
  role: UserRole
}

export interface UpdateProfileInput {
  firstName: string
  lastName: string
  email: string
}

export interface ChangePasswordInput {
  currentPassword: string
  newPassword: string
}

export function getProfile(): Promise<Profile> {
  return apiFetch<Profile>('/account/me')
}

export function updateProfile(input: UpdateProfileInput): Promise<Profile> {
  return apiFetch<Profile>('/account/me', {
    method: 'PUT',
    body: JSON.stringify(input),
  })
}

export function changePassword(input: ChangePasswordInput): Promise<void> {
  return apiFetch<void>('/account/password', {
    method: 'PUT',
    body: JSON.stringify(input),
  })
}
