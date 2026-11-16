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

export function RegisterPage() {
  const { register } = useAuth()
  const navigate = useNavigate()
  const toast = useToast()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [busy, setBusy] = useState(false)

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()
    setBusy(true)
    try {
      await register(email, password)
      toast.success('Konto utworzone. Witaj w FlexDesk!')
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
        <h2 className="text-3xl font-semibold tracking-tight text-fg">Utwórz konto</h2>
        <p className="mt-2 text-base text-muted">Zacznij rezerwować przestrzenie w FlexDesk.</p>
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
        <Field label="Hasło (min. 8 znaków)" className="text-[0.95rem]">
          <Input
            type="password"
            className="h-12 text-base"
            value={password}
            onChange={(event) => setPassword(event.target.value)}
            minLength={8}
            required
            autoComplete="new-password"
          />
        </Field>
        <Button type="submit" disabled={busy} className="mt-2 h-12 w-full text-base">
          {busy ? 'Tworzenie…' : 'Utwórz konto'}
        </Button>
      </form>

      <p className="mt-8 text-center text-base text-muted">
        Masz już konto?{' '}
        <Link to="/login" className="font-medium text-primary hover:underline">
          Zaloguj się
        </Link>
      </p>
    </AuthLayout>
  )
}
