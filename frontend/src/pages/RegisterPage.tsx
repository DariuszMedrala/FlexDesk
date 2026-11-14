import { useState } from 'react'
import type { FormEvent } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext'
import { ErrorBanner } from '../components/ErrorBanner'

export function RegisterPage() {
  const { register } = useAuth()
  const navigate = useNavigate()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState<unknown>(null)
  const [busy, setBusy] = useState(false)

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()
    setError(null)
    setBusy(true)
    try {
      await register(email, password)
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
      <h2>Create account</h2>
      <ErrorBanner error={error} />
      <form onSubmit={handleSubmit} className="form">
        <label>
          Email
          <input type="email" value={email} onChange={(event) => setEmail(event.target.value)} required />
        </label>
        <label>
          Password (min. 8 characters)
          <input
            type="password"
            value={password}
            onChange={(event) => setPassword(event.target.value)}
            minLength={8}
            required
          />
        </label>
        <button type="submit" disabled={busy}>
          {busy ? 'Creating…' : 'Create account'}
        </button>
      </form>
      <p>
        Already registered? <Link to="/login">Sign in</Link>
      </p>
    </div>
  )
}
