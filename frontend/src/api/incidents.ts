import type { Incident, IncidentStatus } from '../types/models'
import { apiFetch } from './client'

export interface CreateIncidentInput {
  workspaceId: string
  description: string
}

export function getIncidents(): Promise<Incident[]> {
  return apiFetch<Incident[]>('/incidents')
}

export function createIncident(input: CreateIncidentInput): Promise<Incident> {
  return apiFetch<Incident>('/incidents', {
    method: 'POST',
    body: JSON.stringify(input),
  })
}

export function updateIncidentStatus(id: string, status: IncidentStatus): Promise<Incident> {
  return apiFetch<Incident>(`/incidents/${id}/status`, {
    method: 'PUT',
    body: JSON.stringify({ status }),
  })
}
