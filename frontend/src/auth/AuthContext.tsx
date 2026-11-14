import { createContext, useContext, useMemo, useState } from 'react'
import type { ReactNode } from 'react'
import { AUTH_STORAGE_KEY } from '../api/client'
import * as authApi from '../api/auth'
import type { AuthResponse, UserRole } from '../types/models'

interface AuthUser {
  email: string
  role: UserRole
}

interface AuthContextValue {
  user: AuthUser | null
  isAdmin: boolean
  login: (email: string, password: string) => Promise<void>
  register: (email: string, password: string) => Promise<void>
  logout: () => void
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined)

function loadStoredAuth(): AuthResponse | null {
  const raw = localStorage.getItem(AUTH_STORAGE_KEY)
  if (!raw) {
    return null
  }

  try {
    const parsed = JSON.parse(raw) as AuthResponse
    if (new Date(parsed.expiresAtUtc).getTime() <= Date.now()) {
      localStorage.removeItem(AUTH_STORAGE_KEY)
      return null
    }
    return parsed
  } catch {
    localStorage.removeItem(AUTH_STORAGE_KEY)
    return null
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [auth, setAuth] = useState<AuthResponse | null>(loadStoredAuth)

  const value = useMemo<AuthContextValue>(() => {
    function persist(response: AuthResponse) {
      localStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify(response))
      setAuth(response)
    }

    return {
      user: auth ? { email: auth.email, role: auth.role } : null,
      isAdmin: auth?.role === 'Admin',
      login: async (email, password) => persist(await authApi.login(email, password)),
      register: async (email, password) => persist(await authApi.register(email, password)),
      logout: () => {
        localStorage.removeItem(AUTH_STORAGE_KEY)
        setAuth(null)
      },
    }
  }, [auth])

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext)
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider')
  }
  return context
}
