import { useState } from 'react'
import type { FormEvent } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext'
import { AuthLayout } from '../components/AuthLayout'
import { Button } from '../components/ui/Button'
import { Field } from '../components/ui/Field'
import { Input } from '../components/ui/Input'
import { getErrorMessage } from '../lib/errors'
import { useToast } from '../toast/ToastContext'

export function LoginPage() {
  const { login } = useAuth()
  const navigate = useNavigate()
  const toast = useToast()
  const [email, setEmail] = useState('admin@flexdesk.local')
  const [password, setPassword] = useState('Admin123!')
  const [busy, setBusy] = useState(false)

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()
    setBusy(true)
    try {
      await login(email, password)
      navigate('/workspaces')
    } catch (error) {
      toast.error(getErrorMessage(error))
    } finally {
      setBusy(false)
    }
  }

  return (
    <AuthLayout>
      <div className="mb-8">
        <h2 className="text-2xl font-semibold text-fg">Zaloguj się</h2>
        <p className="mt-1 text-sm text-muted">Witaj ponownie w FlexDesk.</p>
      </div>

      <form onSubmit={handleSubmit} className="flex flex-col gap-4">
        <Field label="Email">
          <Input type="email" value={email} onChange={(event) => setEmail(event.target.value)} required autoComplete="email" />
        </Field>
        <Field label="Hasło">
          <Input
            type="password"
            value={password}
            onChange={(event) => setPassword(event.target.value)}
            required
            autoComplete="current-password"
          />
        </Field>
        <Button type="submit" disabled={busy} className="mt-2 w-full">
          {busy ? 'Logowanie…' : 'Zaloguj się'}
        </Button>
      </form>

      <p className="mt-6 text-center text-sm text-muted">
        Nie masz konta?{' '}
        <Link to="/register" className="font-medium text-primary hover:underline">
          Zarejestruj się
        </Link>
      </p>
    </AuthLayout>
  )
}
