import type { PagedResult, User } from '../types/models'
import { apiFetch } from './client'

export interface UserQuery {
  page?: number
  pageSize?: number
}

function buildQuery(query: UserQuery): string {
  const params = new URLSearchParams()
  if (query.page) params.set('page', String(query.page))
  if (query.pageSize) params.set('pageSize', String(query.pageSize))
  const value = params.toString()
  return value ? `?${value}` : ''
}

export function getUsers(query: UserQuery): Promise<PagedResult<User>> {
  return apiFetch<PagedResult<User>>(`/users${buildQuery(query)}`)
}
