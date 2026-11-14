const API_BASE = import.meta.env.VITE_API_URL ?? ''
const AUTH_STORAGE_KEY = 'flexdesk_auth'

export class ApiError extends Error {
  readonly status: number
  readonly errors?: Record<string, string[]>

  constructor(status: number, message: string, errors?: Record<string, string[]>) {
    super(message)
    this.status = status
    this.errors = errors
  }
}

function readToken(): string | null {
  const raw = localStorage.getItem(AUTH_STORAGE_KEY)
  if (!raw) {
    return null
  }

  try {
    return (JSON.parse(raw) as { token?: string }).token ?? null
  } catch {
    return null
  }
}

export async function apiFetch<T>(path: string, options: RequestInit = {}): Promise<T> {
  const headers = new Headers(options.headers)
  if (options.body && !headers.has('Content-Type')) {
    headers.set('Content-Type', 'application/json')
  }

  const token = readToken()
  if (token) {
    headers.set('Authorization', `Bearer ${token}`)
  }

  const response = await fetch(`${API_BASE}/api${path}`, { ...options, headers })

  if (response.status === 204) {
    return undefined as T
  }

  const text = await response.text()
  const data = text ? JSON.parse(text) : null

  if (!response.ok) {
    const problem = data as { title?: string; detail?: string; errors?: Record<string, string[]> } | null
    const message = problem?.detail ?? problem?.title ?? `Request failed with status ${response.status}`
    throw new ApiError(response.status, message, problem?.errors)
  }

  return data as T
}

export { AUTH_STORAGE_KEY }
