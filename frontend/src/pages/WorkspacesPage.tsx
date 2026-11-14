import { useCallback, useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import { useAuth } from '../auth/AuthContext'
import { ErrorBanner } from '../components/ErrorBanner'
import { createReservation } from '../api/reservations'
import {
  createWorkspace,
  deleteWorkspace,
  getWorkspaces,
  updateWorkspace,
} from '../api/workspaces'
import type { WorkspaceInput, WorkspaceQuery } from '../api/workspaces'
import type { PagedResult, Workspace, WorkspaceType } from '../types/models'

const WORKSPACE_TYPES: WorkspaceType[] = ['HotDesk', 'MeetingRoom', 'Office']
const PAGE_SIZE = 6

const emptyForm: WorkspaceInput = {
  name: '',
  type: 'HotDesk',
  capacity: 1,
  pricePerHour: 10,
  isActive: true,
}

export function WorkspacesPage() {
  const { isAdmin } = useAuth()
  const [query, setQuery] = useState<WorkspaceQuery>({ page: 1, pageSize: PAGE_SIZE, type: '', minCapacity: '', search: '' })
  const [data, setData] = useState<PagedResult<Workspace> | null>(null)
  const [error, setError] = useState<unknown>(null)
  const [notice, setNotice] = useState<string | null>(null)

  const [form, setForm] = useState<WorkspaceInput>(emptyForm)
  const [editingId, setEditingId] = useState<string | null>(null)

  const [reserveId, setReserveId] = useState<string | null>(null)
  const [reserveStart, setReserveStart] = useState('')
  const [reserveEnd, setReserveEnd] = useState('')

  const load = useCallback(async () => {
    setError(null)
    try {
      setData(await getWorkspaces(query))
    } catch (loadError) {
      setError(loadError)
    }
  }, [query])

  useEffect(() => {
    void load()
  }, [load])

  async function handleSaveWorkspace(event: FormEvent) {
    event.preventDefault()
    setError(null)
    setNotice(null)
    try {
      if (editingId) {
        await updateWorkspace(editingId, form)
        setNotice('Workspace updated.')
      } else {
        await createWorkspace(form)
        setNotice('Workspace created.')
      }
      setForm(emptyForm)
      setEditingId(null)
      await load()
    } catch (saveError) {
      setError(saveError)
    }
  }

  function startEdit(workspace: Workspace) {
    setEditingId(workspace.id)
    setForm({
      name: workspace.name,
      type: workspace.type,
      capacity: workspace.capacity,
      pricePerHour: workspace.pricePerHour,
      isActive: workspace.isActive,
    })
  }

  async function handleDelete(id: string) {
    setError(null)
    setNotice(null)
    try {
      await deleteWorkspace(id)
      setNotice('Workspace deleted.')
      await load()
    } catch (deleteError) {
      setError(deleteError)
    }
  }

  async function handleReserve(event: FormEvent) {
    event.preventDefault()
    if (!reserveId) {
      return
    }
    setError(null)
    setNotice(null)
    try {
      await createReservation({
        workspaceId: reserveId,
        startTime: new Date(reserveStart).toISOString(),
        endTime: new Date(reserveEnd).toISOString(),
      })
      setNotice('Reservation created. Go to Reservations to pay.')
      setReserveId(null)
      setReserveStart('')
      setReserveEnd('')
    } catch (reserveError) {
      setError(reserveError)
    }
  }

  return (
    <div>
      <h1>Workspaces</h1>
      <ErrorBanner error={error} />
      {notice && <div className="banner banner-success">{notice}</div>}

      <form className="filters" onSubmit={(event) => event.preventDefault()}>
        <input
          placeholder="Search by name"
          value={query.search ?? ''}
          onChange={(event) => setQuery({ ...query, page: 1, search: event.target.value })}
        />
        <select value={query.type} onChange={(event) => setQuery({ ...query, page: 1, type: event.target.value as WorkspaceType | '' })}>
          <option value="">All types</option>
          {WORKSPACE_TYPES.map((type) => (
            <option key={type} value={type}>
              {type}
            </option>
          ))}
        </select>
        <input
          type="number"
          min={1}
          placeholder="Min capacity"
          value={query.minCapacity === '' ? '' : query.minCapacity}
          onChange={(event) =>
            setQuery({ ...query, page: 1, minCapacity: event.target.value === '' ? '' : Number(event.target.value) })
          }
        />
      </form>

      {isAdmin && (
        <form className="form card" onSubmit={handleSaveWorkspace}>
          <h3>{editingId ? 'Edit workspace' : 'New workspace'}</h3>
          <div className="form-row">
            <input
              placeholder="Name"
              value={form.name}
              onChange={(event) => setForm({ ...form, name: event.target.value })}
              required
            />
            <select value={form.type} onChange={(event) => setForm({ ...form, type: event.target.value as WorkspaceType })}>
              {WORKSPACE_TYPES.map((type) => (
                <option key={type} value={type}>
                  {type}
                </option>
              ))}
            </select>
            <input
              type="number"
              min={1}
              value={form.capacity}
              onChange={(event) => setForm({ ...form, capacity: Number(event.target.value) })}
              required
            />
            <input
              type="number"
              min={0}
              step={0.5}
              value={form.pricePerHour}
              onChange={(event) => setForm({ ...form, pricePerHour: Number(event.target.value) })}
              required
            />
            <label className="checkbox">
              <input
                type="checkbox"
                checked={form.isActive ?? true}
                onChange={(event) => setForm({ ...form, isActive: event.target.checked })}
              />
              Active
            </label>
          </div>
          <div className="form-actions">
            <button type="submit">{editingId ? 'Save' : 'Create'}</button>
            {editingId && (
              <button
                type="button"
                className="secondary"
                onClick={() => {
                  setEditingId(null)
                  setForm(emptyForm)
                }}
              >
                Cancel
              </button>
            )}
          </div>
        </form>
      )}

      <table className="table">
        <thead>
          <tr>
            <th>Name</th>
            <th>Type</th>
            <th>Capacity</th>
            <th>Price/h</th>
            <th>Status</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          {data?.items.map((workspace) => (
            <tr key={workspace.id}>
              <td>{workspace.name}</td>
              <td>{workspace.type}</td>
              <td>{workspace.capacity}</td>
              <td>{workspace.pricePerHour.toFixed(2)}</td>
              <td>{workspace.isActive ? 'Active' : 'Inactive'}</td>
              <td className="row-actions">
                {workspace.isActive && (
                  <button type="button" onClick={() => setReserveId(workspace.id)}>
                    Reserve
                  </button>
                )}
                {isAdmin && (
                  <>
                    <button type="button" className="secondary" onClick={() => startEdit(workspace)}>
                      Edit
                    </button>
                    <button type="button" className="danger" onClick={() => handleDelete(workspace.id)}>
                      Delete
                    </button>
                  </>
                )}
              </td>
            </tr>
          ))}
          {data && data.items.length === 0 && (
            <tr>
              <td colSpan={6}>No workspaces match the filters.</td>
            </tr>
          )}
        </tbody>
      </table>

      {data && (
        <div className="pagination">
          <button type="button" disabled={data.page <= 1} onClick={() => setQuery({ ...query, page: data.page - 1 })}>
            Previous
          </button>
          <span>
            Page {data.page} of {Math.max(1, data.totalPages)} · {data.totalCount} total
          </span>
          <button
            type="button"
            disabled={data.page >= data.totalPages}
            onClick={() => setQuery({ ...query, page: data.page + 1 })}
          >
            Next
          </button>
        </div>
      )}

      {reserveId && (
        <form className="form card" onSubmit={handleReserve}>
          <h3>Reserve workspace</h3>
          <div className="form-row">
            <label>
              Start
              <input type="datetime-local" value={reserveStart} onChange={(event) => setReserveStart(event.target.value)} required />
            </label>
            <label>
              End
              <input type="datetime-local" value={reserveEnd} onChange={(event) => setReserveEnd(event.target.value)} required />
            </label>
          </div>
          <div className="form-actions">
            <button type="submit">Confirm reservation</button>
            <button type="button" className="secondary" onClick={() => setReserveId(null)}>
              Cancel
            </button>
          </div>
        </form>
      )}
    </div>
  )
}
