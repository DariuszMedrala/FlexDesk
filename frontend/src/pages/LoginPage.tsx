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
        <h2 className="text-3xl font-semibold tracking-tight text-fg">Zaloguj się</h2>
        <p className="mt-2 text-base text-muted">Witaj ponownie w FlexDesk.</p>
      </div>

      <form onSubmit={handleSubmit} className="flex flex-col gap-5">
        <Field label="Email" className="text-[0.95rem]">
          <Input
            type="email"
            className="h-12 text-base"
            value={email}
            onChange={(event) => setEmail(event.target.value)}
            required
            autoComplete="email"
          />
        </Field>
        <Field label="Hasło" className="text-[0.95rem]">
          <Input
            type="password"
            className="h-12 text-base"
            value={password}
            onChange={(event) => setPassword(event.target.value)}
            required
            autoComplete="current-password"
          />
        </Field>
        <Button type="submit" disabled={busy} className="mt-2 h-12 w-full text-base">
          {busy ? 'Logowanie…' : 'Zaloguj się'}
        </Button>
      </form>

      <p className="mt-8 text-center text-base text-muted">
        Nie masz konta?{' '}
        <Link to="/register" className="font-medium text-primary hover:underline">
          Zarejestruj się
        </Link>
      </p>
    </AuthLayout>
  )
}
