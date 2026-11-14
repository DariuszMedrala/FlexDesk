import { useState } from 'react'
import type { FormEvent } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext'
import { ErrorBanner } from '../components/ErrorBanner'

export function LoginPage() {
  const { login } = useAuth()
  const navigate = useNavigate()
  const [email, setEmail] = useState('admin@flexdesk.local')
  const [password, setPassword] = useState('Admin123!')
  const [error, setError] = useState<unknown>(null)
  const [busy, setBusy] = useState(false)

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()
    setError(null)
    setBusy(true)
    try {
      await login(email, password)
      navigate('/workspaces')
    } catch (submitError) {
      setError(submitError)
    } finally {
      setBusy(false)
    }
  }

  return (
    <div className="auth-card">
      <h1>FlexDesk</h1>
      <h2>Sign in</h2>
      <ErrorBanner error={error} />
      <form onSubmit={handleSubmit} className="form">
        <label>
          Email
          <input type="email" value={email} onChange={(event) => setEmail(event.target.value)} required />
        </label>
        <label>
          Password
          <input type="password" value={password} onChange={(event) => setPassword(event.target.value)} required />
        </label>
        <button type="submit" disabled={busy}>
          {busy ? 'Signing in…' : 'Sign in'}
        </button>
      </form>
      <p>
        No account? <Link to="/register">Create one</Link>
      </p>
    </div>
  )
}
