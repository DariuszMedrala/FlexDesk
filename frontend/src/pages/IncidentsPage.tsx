import { useCallback, useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import { TriangleAlert } from 'lucide-react'
import { useAuth } from '../auth/AuthContext'
import { createIncident, getIncidents, updateIncidentStatus } from '../api/incidents'
import { getWorkspaces } from '../api/workspaces'
import { Badge } from '../components/ui/Badge'
import { Button } from '../components/ui/Button'
import { Card } from '../components/ui/Card'
import { EmptyState } from '../components/ui/EmptyState'
import { Field } from '../components/ui/Field'
import { Input } from '../components/ui/Input'
import { Select } from '../components/ui/Select'
import { PageLoader } from '../components/ui/Spinner'
import { getErrorMessage } from '../lib/errors'
import { useToast } from '../toast/ToastContext'
import type { Incident, IncidentStatus, Workspace } from '../types/models'

const INCIDENT_STATUSES: IncidentStatus[] = ['Reported', 'InProgress', 'Resolved']

const statusTone: Record<IncidentStatus, 'warning' | 'info' | 'success'> = {
  Reported: 'warning',
  InProgress: 'info',
  Resolved: 'success',
}

function formatDate(value: string): string {
  return new Date(value).toLocaleString()
}

export function IncidentsPage() {
  const { isAdmin } = useAuth()
  const toast = useToast()

  const [incidents, setIncidents] = useState<Incident[]>([])
  const [workspaces, setWorkspaces] = useState<Workspace[]>([])
  const [loading, setLoading] = useState(true)

  const [workspaceId, setWorkspaceId] = useState('')
  const [description, setDescription] = useState('')
  const [reporting, setReporting] = useState(false)

  const load = useCallback(async () => {
    setLoading(true)
    try {
      const [incidentList, workspacePage] = await Promise.all([
        getIncidents(),
        getWorkspaces({ page: 1, pageSize: 100 }),
      ])
      setIncidents(incidentList)
      setWorkspaces(workspacePage.items)
      setWorkspaceId((current) => current || workspacePage.items[0]?.id || '')
    } catch (error) {
      toast.error(getErrorMessage(error))
    } finally {
      setLoading(false)
    }
  }, [toast])

  useEffect(() => {
    void load()
  }, [load])

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
        <p className="mt-1 text-sm text-muted">Zgłaszaj problemy i śledź ich status.</p>
      </div>

      <Card className="p-5">
        <h2 className="mb-4 font-semibold text-fg">Zgłoś usterkę</h2>
        <form onSubmit={handleReport} className="flex flex-col gap-4 sm:flex-row sm:items-end">
          <Field label="Przestrzeń" className="sm:w-56">
            <Select value={workspaceId} onChange={(event) => setWorkspaceId(event.target.value)} required>
              {workspaces.map((workspace) => (
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
      </Card>

      {loading ? (
        <PageLoader />
      ) : incidents.length === 0 ? (
        <Card>
          <EmptyState icon={<TriangleAlert size={22} />} title="Brak zgłoszeń" description="Nie zgłoszono jeszcze żadnej usterki." />
        </Card>
      ) : (
        <Card className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-border text-left text-xs font-medium text-muted">
                <th className="px-5 py-3">Przestrzeń</th>
                <th className="px-5 py-3">Opis</th>
                <th className="px-5 py-3">Status</th>
                <th className="px-5 py-3">Zgłoszono</th>
                {isAdmin && <th className="px-5 py-3 text-right">Zmień status</th>}
              </tr>
            </thead>
            <tbody>
              {incidents.map((incident) => (
                <tr key={incident.id} className="border-b border-border last:border-0">
                  <td className="px-5 py-3 font-medium text-fg">{incident.workspaceName}</td>
                  <td className="px-5 py-3 text-muted">{incident.description}</td>
                  <td className="px-5 py-3">
                    <Badge tone={statusTone[incident.status]}>{incident.status}</Badge>
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
                              {status}
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
      )}
    </div>
  )
}
