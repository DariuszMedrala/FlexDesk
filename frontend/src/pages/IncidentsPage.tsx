import { useCallback, useEffect, useMemo, useState } from 'react'
import type { FormEvent } from 'react'
import { TriangleAlert } from 'lucide-react'
import { useAuth } from '../auth/AuthContext'
import { createIncident, getIncidents, updateIncidentStatus } from '../api/incidents'
import { getReservations } from '../api/reservations'
import { Badge } from '../components/ui/Badge'
import { Button } from '../components/ui/Button'
import { Card } from '../components/ui/Card'
import { EmptyState } from '../components/ui/EmptyState'
import { Field } from '../components/ui/Field'
import { Input } from '../components/ui/Input'
import { Select } from '../components/ui/Select'
import { PageLoader } from '../components/ui/Spinner'
import { cn } from '../lib/cn'
import { getErrorMessage } from '../lib/errors'
import { useToast } from '../toast/ToastContext'
import type { Incident, IncidentStatus } from '../types/models'

const INCIDENT_STATUSES: IncidentStatus[] = ['Reported', 'InProgress', 'Resolved']

const statusTone: Record<IncidentStatus, 'warning' | 'info' | 'success'> = {
  Reported: 'warning',
  InProgress: 'info',
  Resolved: 'success',
}

const statusLabel: Record<IncidentStatus, string> = {
  Reported: 'Zgłoszona',
  InProgress: 'W toku',
  Resolved: 'Rozwiązana',
}

type StatusFilter = IncidentStatus | 'All'

const statusFilters: { value: StatusFilter; label: string }[] = [
  { value: 'All', label: 'Wszystkie' },
  { value: 'Reported', label: 'Zgłoszone' },
  { value: 'InProgress', label: 'W toku' },
  { value: 'Resolved', label: 'Rozwiązane' },
]

const PAGE_SIZE = 8

interface EligibleWorkspace {
  id: string
  name: string
}

function formatDate(value: string): string {
  return new Date(value).toLocaleString()
}

export function IncidentsPage() {
  const { isAdmin } = useAuth()
  const toast = useToast()

  const [incidents, setIncidents] = useState<Incident[]>([])
  const [eligible, setEligible] = useState<EligibleWorkspace[]>([])
  const [loading, setLoading] = useState(true)

  const [workspaceId, setWorkspaceId] = useState('')
  const [description, setDescription] = useState('')
  const [reporting, setReporting] = useState(false)

  const [filter, setFilter] = useState<StatusFilter>('All')
  const [page, setPage] = useState(1)

  const load = useCallback(async () => {
    setLoading(true)
    try {
      const incidentList = await getIncidents()
      setIncidents(incidentList)

      if (!isAdmin) {
        const reservations = await getReservations()
        const now = Date.now()
        const windowMs = 24 * 60 * 60 * 1000
        const map = new Map<string, string>()
        reservations
          .filter((r) => r.status !== 'Cancelled' && now <= new Date(r.endTime).getTime() + windowMs)
          .forEach((r) => map.set(r.workspaceId, r.workspaceName))
        const options = [...map.entries()].map(([id, name]) => ({ id, name }))
        setEligible(options)
        setWorkspaceId((current) => current || options[0]?.id || '')
      }
    } catch (error) {
      toast.error(getErrorMessage(error))
    } finally {
      setLoading(false)
    }
  }, [isAdmin, toast])

  useEffect(() => {
    void load()
  }, [load])

  const visible = useMemo(
    () => (filter === 'All' ? incidents : incidents.filter((item) => item.status === filter)),
    [incidents, filter],
  )

  const totalPages = Math.max(1, Math.ceil(visible.length / PAGE_SIZE))
  const safePage = Math.min(page, totalPages)
  const pageItems = visible.slice((safePage - 1) * PAGE_SIZE, safePage * PAGE_SIZE)

  async function handleReport(event: FormEvent) {
    event.preventDefault()
    setReporting(true)
    try {
      await createIncident({ workspaceId, description })
      toast.success('Zgłoszenie dodane.')
      setDescription('')
      await load()
    } catch (error) {
      toast.error(getErrorMessage(error))
    } finally {
      setReporting(false)
    }
  }

  async function handleStatusChange(id: string, status: IncidentStatus) {
    try {
      await updateIncidentStatus(id, status)
      toast.success('Status zaktualizowany.')
      await load()
    } catch (error) {
      toast.error(getErrorMessage(error))
    }
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold text-fg">Zgłoszenia usterek</h1>
        <p className="mt-1 text-sm text-muted">
          {isAdmin ? 'Weryfikuj i aktualizuj status zgłoszeń.' : 'Zgłaszaj problemy i śledź ich status.'}
        </p>
      </div>

      {!isAdmin && (
        <Card className="p-5">
          <h2 className="mb-4 font-semibold text-fg">Zgłoś usterkę</h2>
          {eligible.length === 0 ? (
            <p className="text-sm text-muted">
              Zgłoszenie możesz dodać tylko dla przestrzeni z rozpoczętą rezerwacją. Nie masz obecnie takiej rezerwacji.
            </p>
          ) : (
            <form onSubmit={handleReport} className="flex flex-col gap-4 sm:flex-row sm:items-end">
              <Field label="Przestrzeń" className="sm:w-56">
                <Select value={workspaceId} onChange={(event) => setWorkspaceId(event.target.value)} required>
                  {eligible.map((workspace) => (
                    <option key={workspace.id} value={workspace.id}>
                      {workspace.name}
                    </option>
                  ))}
                </Select>
              </Field>
              <Field label="Opis" className="flex-1">
                <Input
                  placeholder="Opisz problem"
                  value={description}
                  onChange={(event) => setDescription(event.target.value)}
                  required
                />
              </Field>
              <Button type="submit" disabled={reporting}>
                {reporting ? 'Wysyłanie…' : 'Zgłoś'}
              </Button>
            </form>
          )}
        </Card>
      )}

      <div className="flex flex-wrap gap-2">
        {statusFilters.map((item) => (
          <button
            key={item.value}
            type="button"
            onClick={() => {
              setFilter(item.value)
              setPage(1)
            }}
            className={cn(
              'rounded-lg border px-3 py-1.5 text-sm font-medium transition-colors',
              filter === item.value
                ? 'border-primary bg-primary text-primary-fg'
                : 'border-border bg-surface text-muted hover:bg-surface-2 hover:text-fg',
            )}
          >
            {item.label}
          </button>
        ))}
      </div>

      {loading ? (
        <PageLoader />
      ) : visible.length === 0 ? (
        <Card>
          <EmptyState icon={<TriangleAlert size={22} />} title="Brak zgłoszeń" description="Nie ma zgłoszeń dla wybranego filtra." />
        </Card>
      ) : (
        <>
          <Card className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-border text-left text-xs font-medium text-muted">
                  <th className="px-5 py-3">Przestrzeń</th>
                  {isAdmin && <th className="px-5 py-3">Zgłaszający</th>}
                  <th className="px-5 py-3">Opis</th>
                  <th className="px-5 py-3">Status</th>
                  <th className="px-5 py-3">Zgłoszono</th>
                  {isAdmin && <th className="px-5 py-3 text-right">Zmień status</th>}
                </tr>
              </thead>
              <tbody>
                {pageItems.map((incident) => (
                  <tr key={incident.id} className="border-b border-border last:border-0">
                    <td className="px-5 py-3 font-medium text-fg">{incident.workspaceName}</td>
                    {isAdmin && <td className="px-5 py-3 text-muted">{incident.userFullName}</td>}
                    <td className="px-5 py-3 text-muted">{incident.description}</td>
                    <td className="px-5 py-3">
                      <Badge tone={statusTone[incident.status]}>{statusLabel[incident.status]}</Badge>
                    </td>
                    <td className="px-5 py-3 text-muted">{formatDate(incident.createdAt)}</td>
                    {isAdmin && (
                      <td className="px-5 py-3">
                        <div className="flex justify-end">
                          <Select
                            className="w-40"
                            value={incident.status}
                            disabled={incident.status === 'Resolved'}
                            onChange={(event) => handleStatusChange(incident.id, event.target.value as IncidentStatus)}
                          >
                            {INCIDENT_STATUSES.map((status) => (
                              <option key={status} value={status}>
                                {statusLabel[status]}
                              </option>
                            ))}
                          </Select>
                        </div>
                      </td>
                    )}
                  </tr>
                ))}
              </tbody>
            </table>
          </Card>

          <div className="flex items-center justify-between">
            <p className="text-sm text-muted">
              Strona {safePage} z {totalPages} · {visible.length} wyników
            </p>
            <div className="flex gap-2">
              <Button size="sm" variant="secondary" disabled={safePage <= 1} onClick={() => setPage(safePage - 1)}>
                Poprzednia
              </Button>
              <Button size="sm" variant="secondary" disabled={safePage >= totalPages} onClick={() => setPage(safePage + 1)}>
                Następna
              </Button>
            </div>
          </div>
        </>
      )}
    </div>
  )
}
