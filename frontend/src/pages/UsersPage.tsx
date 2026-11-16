import { useCallback, useEffect, useState } from 'react'
import { Mail, Users } from 'lucide-react'
import { Navigate } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext'
import { getUsers } from '../api/users'
import { Badge } from '../components/ui/Badge'
import { Button } from '../components/ui/Button'
import { Card } from '../components/ui/Card'
import { EmptyState } from '../components/ui/EmptyState'
import { PageLoader } from '../components/ui/Spinner'
import { getErrorMessage } from '../lib/errors'
import { useToast } from '../toast/ToastContext'
import type { PagedResult, User } from '../types/models'

const PAGE_SIZE = 9

function initials(user: User): string {
  return `${user.firstName.charAt(0)}${user.lastName.charAt(0)}`.toUpperCase()
}

export function UsersPage() {
  const { isAdmin } = useAuth()
  const toast = useToast()
  const [page, setPage] = useState(1)
  const [data, setData] = useState<PagedResult<User> | null>(null)
  const [loading, setLoading] = useState(true)

  const load = useCallback(async () => {
    setLoading(true)
    try {
      setData(await getUsers({ page, pageSize: PAGE_SIZE }))
    } catch (error) {
      toast.error(getErrorMessage(error))
    } finally {
      setLoading(false)
    }
  }, [page, toast])

  useEffect(() => {
    void load()
  }, [load])

  if (!isAdmin) {
    return <Navigate to="/workspaces" replace />
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold text-fg">Użytkownicy</h1>
        <p className="mt-1 text-sm text-muted">Lista zarejestrowanych użytkowników.</p>
      </div>

      {loading ? (
        <PageLoader />
      ) : data && data.items.length > 0 ? (
        <>
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {data.items.map((user) => (
              <Card key={user.id} className="flex flex-col gap-3 p-5">
                <div className="flex items-center gap-3">
                  <div className="flex size-11 items-center justify-center rounded-full bg-primary/10 text-sm font-semibold text-primary">
                    {initials(user)}
                  </div>
                  <div className="min-w-0">
                    <p className="truncate font-semibold text-fg">
                      {user.firstName} {user.lastName}
                    </p>
                    <Badge tone={user.role === 'Admin' ? 'info' : 'neutral'}>
                      {user.role === 'Admin' ? 'Administrator' : 'Użytkownik'}
                    </Badge>
                  </div>
                </div>
                <p className="flex items-center gap-2 truncate text-sm text-muted">
                  <Mail size={15} className="shrink-0" />
                  {user.email}
                </p>
              </Card>
            ))}
          </div>

          <div className="flex items-center justify-between">
            <p className="text-sm text-muted">
              Strona {data.page} z {Math.max(1, data.totalPages)} · {data.totalCount} użytkowników
            </p>
            <div className="flex gap-2">
              <Button size="sm" variant="secondary" disabled={data.page <= 1} onClick={() => setPage(data.page - 1)}>
                Poprzednia
              </Button>
              <Button size="sm" variant="secondary" disabled={data.page >= data.totalPages} onClick={() => setPage(data.page + 1)}>
                Następna
              </Button>
            </div>
          </div>
        </>
      ) : (
        <Card>
          <EmptyState icon={<Users size={22} />} title="Brak użytkowników" />
        </Card>
      )}
    </div>
  )
}
