import { useState } from 'react'
import type { FormEvent } from 'react'
import { KeyRound, UserCog } from 'lucide-react'
import { useAuth } from '../auth/AuthContext'
import { changePassword, updateProfile } from '../api/account'
import { Button } from '../components/ui/Button'
import { Card } from '../components/ui/Card'
import { Field } from '../components/ui/Field'
import { Input } from '../components/ui/Input'
import { getErrorMessage } from '../lib/errors'
import { useToast } from '../toast/ToastContext'

export function SettingsPage() {
  const { user, updateProfile: updateStoredProfile } = useAuth()
  const toast = useToast()

  const [firstName, setFirstName] = useState(user?.firstName ?? '')
  const [lastName, setLastName] = useState(user?.lastName ?? '')
  const [email, setEmail] = useState(user?.email ?? '')
  const [savingProfile, setSavingProfile] = useState(false)

  const [currentPassword, setCurrentPassword] = useState('')
  const [newPassword, setNewPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')
  const [savingPassword, setSavingPassword] = useState(false)

  async function handleProfileSubmit(event: FormEvent) {
    event.preventDefault()
    setSavingProfile(true)
    try {
      const profile = await updateProfile({ firstName, lastName, email })
      updateStoredProfile({ firstName: profile.firstName, lastName: profile.lastName, email: profile.email })
      toast.success('Dane zaktualizowane.')
    } catch (error) {
      toast.error(getErrorMessage(error))
    } finally {
      setSavingProfile(false)
    }
  }

  async function handlePasswordSubmit(event: FormEvent) {
    event.preventDefault()
    if (newPassword !== confirmPassword) {
      toast.error('Nowe hasła nie są takie same.')
      return
    }
    if (newPassword.length < 8) {
      toast.error('Nowe hasło musi mieć co najmniej 8 znaków.')
      return
    }
    setSavingPassword(true)
    try {
      await changePassword({ currentPassword, newPassword })
      toast.success('Hasło zmienione.')
      setCurrentPassword('')
      setNewPassword('')
      setConfirmPassword('')
    } catch (error) {
      toast.error(getErrorMessage(error))
    } finally {
      setSavingPassword(false)
    }
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold text-fg">Ustawienia</h1>
        <p className="mt-1 text-sm text-muted">Zarządzaj swoimi danymi i hasłem.</p>
      </div>

      <div className="grid gap-6 lg:grid-cols-2">
        <Card className="flex flex-col p-6">
          <div className="mb-5 flex items-center gap-3">
            <div className="flex size-10 items-center justify-center rounded-lg bg-primary/10 text-primary">
              <UserCog size={20} />
            </div>
            <div>
              <h2 className="font-semibold text-fg">Dane konta</h2>
              <p className="text-sm text-muted">Zaktualizuj swoje dane osobowe.</p>
            </div>
          </div>
          <form onSubmit={handleProfileSubmit} className="flex flex-1 flex-col gap-4">
            <div className="grid gap-4 sm:grid-cols-2">
              <Field label="Imię">
                <Input value={firstName} onChange={(event) => setFirstName(event.target.value)} required />
              </Field>
              <Field label="Nazwisko">
                <Input value={lastName} onChange={(event) => setLastName(event.target.value)} required />
              </Field>
            </div>
            <Field label="Email">
              <Input type="email" value={email} onChange={(event) => setEmail(event.target.value)} required />
            </Field>
            <div className="mt-auto pt-2">
              <Button type="submit" disabled={savingProfile} className="w-full sm:w-auto">
                {savingProfile ? 'Zapisywanie…' : 'Zapisz dane'}
              </Button>
            </div>
          </form>
        </Card>

        <Card className="flex flex-col p-6">
          <div className="mb-5 flex items-center gap-3">
            <div className="flex size-10 items-center justify-center rounded-lg bg-primary/10 text-primary">
              <KeyRound size={20} />
            </div>
            <div>
              <h2 className="font-semibold text-fg">Zmiana hasła</h2>
              <p className="text-sm text-muted">Podaj obecne hasło i nowe dwukrotnie.</p>
            </div>
          </div>
          <form onSubmit={handlePasswordSubmit} className="flex flex-1 flex-col gap-4">
            <Field label="Obecne hasło">
              <Input
                type="password"
                value={currentPassword}
                onChange={(event) => setCurrentPassword(event.target.value)}
                required
                autoComplete="current-password"
              />
            </Field>
            <Field label="Nowe hasło (min. 8 znaków)">
              <Input
                type="password"
                value={newPassword}
                onChange={(event) => setNewPassword(event.target.value)}
                minLength={8}
                required
                autoComplete="new-password"
              />
            </Field>
            <Field label="Powtórz nowe hasło">
              <Input
                type="password"
                value={confirmPassword}
                onChange={(event) => setConfirmPassword(event.target.value)}
                minLength={8}
                required
                autoComplete="new-password"
              />
            </Field>
            <div className="mt-auto pt-2">
              <Button type="submit" disabled={savingPassword} className="w-full sm:w-auto">
                {savingPassword ? 'Zapisywanie…' : 'Zmień hasło'}
              </Button>
            </div>
          </form>
        </Card>
      </div>
    </div>
  )
}
