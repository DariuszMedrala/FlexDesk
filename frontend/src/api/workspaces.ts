import type { PagedResult, Workspace, WorkspaceType } from '../types/models'
import { apiFetch } from './client'

export interface WorkspaceQuery {
  page?: number
  pageSize?: number
  type?: WorkspaceType | ''
  minCapacity?: number | ''
  search?: string
}

export interface WorkspaceInput {
  name: string
  type: WorkspaceType
  capacity: number
  pricePerHour: number
  isActive?: boolean
}

function buildQuery(query: WorkspaceQuery): string {
  const params = new URLSearchParams()
  if (query.page) params.set('page', String(query.page))
  if (query.pageSize) params.set('pageSize', String(query.pageSize))
  if (query.type) params.set('type', query.type)
  if (query.minCapacity) params.set('minCapacity', String(query.minCapacity))
  if (query.search) params.set('search', query.search)
  const value = params.toString()
  return value ? `?${value}` : ''
}

export function getWorkspaces(query: WorkspaceQuery): Promise<PagedResult<Workspace>> {
  return apiFetch<PagedResult<Workspace>>(`/workspaces${buildQuery(query)}`)
}

export function createWorkspace(input: WorkspaceInput): Promise<Workspace> {
  return apiFetch<Workspace>('/workspaces', {
    method: 'POST',
    body: JSON.stringify(input),
  })
}

export function updateWorkspace(id: string, input: WorkspaceInput): Promise<Workspace> {
  return apiFetch<Workspace>(`/workspaces/${id}`, {
    method: 'PUT',
    body: JSON.stringify(input),
  })
}

export function deleteWorkspace(id: string): Promise<void> {
  return apiFetch<void>(`/workspaces/${id}`, { method: 'DELETE' })
}
