import { useCallback, useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import { useAuth } from '../auth/AuthContext'
import { ErrorBanner } from '../components/ErrorBanner'
import { createIncident, getIncidents, updateIncidentStatus } from '../api/incidents'
import { getWorkspaces } from '../api/workspaces'
import type { Incident, IncidentStatus, Workspace } from '../types/models'

const INCIDENT_STATUSES: IncidentStatus[] = ['Reported', 'InProgress', 'Resolved']

function formatDate(value: string): string {
  return new Date(value).toLocaleString()
}

export function IncidentsPage() {
  const { isAdmin } = useAuth()
  const [incidents, setIncidents] = useState<Incident[]>([])
  const [workspaces, setWorkspaces] = useState<Workspace[]>([])
  const [workspaceId, setWorkspaceId] = useState('')
  const [description, setDescription] = useState('')
  const [error, setError] = useState<unknown>(null)
  const [notice, setNotice] = useState<string | null>(null)

  const load = useCallback(async () => {
    setError(null)
    try {
      const [incidentList, workspacePage] = await Promise.all([
        getIncidents(),
        getWorkspaces({ page: 1, pageSize: 100 }),
      ])
      setIncidents(incidentList)
      setWorkspaces(workspacePage.items)
      if (!workspaceId && workspacePage.items.length > 0) {
        setWorkspaceId(workspacePage.items[0].id)
      }
    } catch (loadError) {
      setError(loadError)
    }
  }, [workspaceId])

  useEffect(() => {
    void load()
  }, [load])

  async function handleReport(event: FormEvent) {
    event.preventDefault()
    setError(null)
    setNotice(null)
    try {
      await createIncident({ workspaceId, description })
      setNotice('Incident reported.')
      setDescription('')
      await load()
    } catch (reportError) {
      setError(reportError)
    }
  }

  async function handleStatusChange(id: string, status: IncidentStatus) {
    setError(null)
    setNotice(null)
    try {
      await updateIncidentStatus(id, status)
      setNotice('Incident status updated.')
      await load()
    } catch (statusError) {
      setError(statusError)
    }
  }

  return (
    <div>
      <h1>Incidents</h1>
      <ErrorBanner error={error} />
      {notice && <div className="banner banner-success">{notice}</div>}

      <form className="form card" onSubmit={handleReport}>
        <h3>Report an issue</h3>
        <div className="form-row">
          <select value={workspaceId} onChange={(event) => setWorkspaceId(event.target.value)} required>
            {workspaces.map((workspace) => (
              <option key={workspace.id} value={workspace.id}>
                {workspace.name}
              </option>
            ))}
          </select>
          <input
            placeholder="Describe the problem"
            value={description}
            onChange={(event) => setDescription(event.target.value)}
            required
          />
        </div>
        <div className="form-actions">
          <button type="submit">Report</button>
        </div>
      </form>

      <table className="table">
        <thead>
          <tr>
            <th>Workspace</th>
            <th>Description</th>
            <th>Status</th>
            <th>Reported</th>
            {isAdmin && <th></th>}
          </tr>
        </thead>
        <tbody>
          {incidents.map((incident) => (
            <tr key={incident.id}>
              <td>{incident.workspaceName}</td>
              <td>{incident.description}</td>
              <td>
                <span className={`badge badge-${incident.status.toLowerCase()}`}>{incident.status}</span>
              </td>
              <td>{formatDate(incident.createdAt)}</td>
              {isAdmin && (
                <td>
                  <select
                    value={incident.status}
                    disabled={incident.status === 'Resolved'}
                    onChange={(event) => handleStatusChange(incident.id, event.target.value as IncidentStatus)}
                  >
                    {INCIDENT_STATUSES.map((status) => (
                      <option key={status} value={status}>
                        {status}
                      </option>
                    ))}
                  </select>
                </td>
              )}
            </tr>
          ))}
          {incidents.length === 0 && (
            <tr>
              <td colSpan={isAdmin ? 5 : 4}>No incidents reported.</td>
            </tr>
          )}
        </tbody>
      </table>
    </div>
  )
}
