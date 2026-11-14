import type { Payment, PaymentMethod, Reservation } from '../types/models'
import { apiFetch } from './client'

export interface CreateReservationInput {
  workspaceId: string
  startTime: string
  endTime: string
}

export function getReservations(): Promise<Reservation[]> {
  return apiFetch<Reservation[]>('/reservations')
}

export function createReservation(input: CreateReservationInput): Promise<Reservation> {
  return apiFetch<Reservation>('/reservations', {
    method: 'POST',
    body: JSON.stringify(input),
  })
}

export function payReservation(id: string, method: PaymentMethod): Promise<Payment> {
  return apiFetch<Payment>(`/reservations/${id}/pay`, {
    method: 'POST',
    body: JSON.stringify({ method }),
  })
}

export function cancelReservation(id: string): Promise<Reservation> {
  return apiFetch<Reservation>(`/reservations/${id}/cancel`, { method: 'POST' })
}
