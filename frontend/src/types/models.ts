export type WorkspaceType = 'HotDesk' | 'MeetingRoom' | 'Office'
export type ReservationStatus = 'Pending' | 'Confirmed' | 'Cancelled' | 'Completed'
export type IncidentStatus = 'Reported' | 'InProgress' | 'Resolved'
export type PaymentMethod = 'Card' | 'BankTransfer' | 'Cash'
export type UserRole = 'User' | 'Admin'

export interface Workspace {
  id: string
  name: string
  type: WorkspaceType
  capacity: number
  pricePerHour: number
  isActive: boolean
  city: string
  street: string
  buildingNumber: string
  floor: number
}

export interface PagedResult<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}

export interface Reservation {
  id: string
  workspaceId: string
  workspaceName: string
  userId: string
  userFullName: string
  startTime: string
  endTime: string
  totalPrice: number
  status: ReservationStatus
  createdAt: string
}

export interface Payment {
  id: string
  reservationId: string
  amount: number
  paymentDate: string
  method: PaymentMethod
  isSuccessful: boolean
}

export interface Incident {
  id: string
  workspaceId: string
  workspaceName: string
  userId: string
  userFullName: string
  description: string
  status: IncidentStatus
  createdAt: string
  resolvedAt: string | null
}

export interface User {
  id: string
  email: string
  firstName: string
  lastName: string
  role: UserRole
  createdAt: string
}

export interface AuthResponse {
  token: string
  expiresAtUtc: string
  email: string
  role: UserRole
  firstName: string
  lastName: string
}
