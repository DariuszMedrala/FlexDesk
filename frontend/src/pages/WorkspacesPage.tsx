import { useCallback, useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import type { LucideIcon } from 'lucide-react'
import {
  Building2,
  CalendarPlus,
  LayoutGrid,
  MapPin,
  Monitor,
  Pencil,
  Plus,
  Search,
  Trash2,
  Users,
} from 'lucide-react'
import { useAuth } from '../auth/AuthContext'
import { createReservation } from '../api/reservations'
import {
  createWorkspace,
  deleteWorkspace,
  getWorkspaces,
  updateWorkspace,
} from '../api/workspaces'
import type { WorkspaceInput, WorkspaceQuery } from '../api/workspaces'
import { Badge } from '../components/ui/Badge'
import { Button } from '../components/ui/Button'
import { Card } from '../components/ui/Card'
import { EmptyState } from '../components/ui/EmptyState'
import { Field } from '../components/ui/Field'
import { Input } from '../components/ui/Input'
import { Modal } from '../components/ui/Modal'
import { Select } from '../components/ui/Select'
import { PageLoader } from '../components/ui/Spinner'
import { Switch } from '../components/ui/Switch'
import { cn } from '../lib/cn'
import { getErrorMessage } from '../lib/errors'
import { useToast } from '../toast/ToastContext'
import type { PagedResult, Workspace, WorkspaceType } from '../types/models'

interface TypeMeta {
  label: string
  icon: LucideIcon
  iconClass: string
  accent: string
}

const typeMeta: Record<WorkspaceType, TypeMeta> = {
  HotDesk: { label: 'Biurko', icon: Monitor, iconClass: 'bg-blue-500/10 text-blue-500', accent: 'border-l-blue-500' },
  MeetingRoom: {
    label: 'Sala konferencyjna',
    icon: Users,
    iconClass: 'bg-amber-500/10 text-amber-500',
    accent: 'border-l-amber-500',
  },
  Office: { label: 'Biuro', icon: Building2, iconClass: 'bg-emerald-500/10 text-emerald-500', accent: 'border-l-emerald-500' },
}

const WORKSPACE_TYPES: WorkspaceType[] = ['HotDesk', 'MeetingRoom', 'Office']
const PAGE_SIZE = 6

const emptyForm: WorkspaceInput = {
  name: '',
  type: 'HotDesk',
  capacity: 1,
  pricePerHour: 10,
  isActive: true,
  city: '',
  street: '',
  buildingNumber: '',
  floor: 0,
}

function floorLabel(floor: number): string {
  return floor === 0 ? 'parter' : `piętro ${floor}`
}

function locationLine(workspace: Workspace): string {
  return `${workspace.city}, ul. ${workspace.street} ${workspace.buildingNumber} · ${floorLabel(workspace.floor)}`
}

function previewPrice(pricePerHour: number, start: string, end: string): number | null {
  if (!start || !end) {
    return null
  }
  const startMs = new Date(start).getTime()
  const endMs = new Date(end).getTime()
  if (Number.isNaN(startMs) || Number.isNaN(endMs) || endMs <= startMs) {
    return null
  }
  const hours = (endMs - startMs) / 3_600_000
  let total = pricePerHour * hours
  if (hours >= 8) {
    total *= 0.9
  }
  return Math.round(total * 100) / 100
}

export function WorkspacesPage() {
  const { isAdmin } = useAuth()
  const toast = useToast()

  const [query, setQuery] = useState<WorkspaceQuery>({ page: 1, pageSize: PAGE_SIZE, type: '', minCapacity: '', search: '' })
  const [data, setData] = useState<PagedResult<Workspace> | null>(null)
  const [loading, setLoading] = useState(true)

  const [formModal, setFormModal] = useState<{ workspace: Workspace | null } | null>(null)
  const [form, setForm] = useState<WorkspaceInput>(emptyForm)
  const [saving, setSaving] = useState(false)

  const [reserveTarget, setReserveTarget] = useState<Workspace | null>(null)
  const [reserveStart, setReserveStart] = useState('')
  const [reserveEnd, setReserveEnd] = useState('')
  const [reserving, setReserving] = useState(false)

  const [deleteTarget, setDeleteTarget] = useState<Workspace | null>(null)

  const load = useCallback(async () => {
    setLoading(true)
    try {
      setData(await getWorkspaces(query))
    } catch (error) {
      toast.error(getErrorMessage(error))
    } finally {
      setLoading(false)
    }
  }, [query, toast])

  useEffect(() => {
    void load()
  }, [load])

  function openCreate() {
    setForm(emptyForm)
    setFormModal({ workspace: null })
  }

  function openEdit(workspace: Workspace) {
    setForm({
      name: workspace.name,
      type: workspace.type,
      capacity: workspace.capacity,
      pricePerHour: workspace.pricePerHour,
      isActive: workspace.isActive,
      city: workspace.city,
      street: workspace.street,
      buildingNumber: workspace.buildingNumber,
      floor: workspace.floor,
    })
    setFormModal({ workspace })
  }

  async function handleSave(event: FormEvent) {
    event.preventDefault()
    setSaving(true)
    try {
      if (formModal?.workspace) {
        await updateWorkspace(formModal.workspace.id, form)
        toast.success('Przestrzeń zaktualizowana.')
      } else {
        await createWorkspace(form)
        toast.success('Przestrzeń utworzona.')
      }
      setFormModal(null)
      await load()
    } catch (error) {
      toast.error(getErrorMessage(error))
    } finally {
      setSaving(false)
    }
  }

  async function handleDelete() {
    if (!deleteTarget) {
      return
    }
    try {
      await deleteWorkspace(deleteTarget.id)
      toast.success('Przestrzeń usunięta.')
      setDeleteTarget(null)
      await load()
    } catch (error) {
      toast.error(getErrorMessage(error))
    }
  }

  async function handleReserve(event: FormEvent) {
    event.preventDefault()
    if (!reserveTarget) {
      return
    }
    setReserving(true)
    try {
      await createReservation({
        workspaceId: reserveTarget.id,
        startTime: new Date(reserveStart).toISOString(),
        endTime: new Date(reserveEnd).toISOString(),
      })
      toast.success('Rezerwacja utworzona. Opłać ją w zakładce Rezerwacje.')
      setReserveTarget(null)
      setReserveStart('')
      setReserveEnd('')
    } catch (error) {
      toast.error(getErrorMessage(error))
    } finally {
      setReserving(false)
    }
  }

  const price = reserveTarget ? previewPrice(reserveTarget.pricePerHour, reserveStart, reserveEnd) : null

  return (
    <div className="space-y-6">
      <div className="flex items-start justify-between gap-4">
        <div>
          <h1 className="text-2xl font-semibold text-fg">Przestrzenie</h1>
          <p className="mt-1 text-sm text-muted">
            {isAdmin
              ? 'Twórz, edytuj i dezaktywuj przestrzenie w katalogu.'
              : 'Przeglądaj dostępne przestrzenie i rezerwuj je.'}
          </p>
        </div>
        {isAdmin && (
          <Button onClick={openCreate}>
            <Plus size={16} />
            Nowa przestrzeń
          </Button>
        )}
      </div>

      <Card className="flex flex-col gap-3 p-4 sm:flex-row sm:items-center">
        <div className="relative flex-1">
          <Search size={16} className="absolute top-1/2 left-3 -translate-y-1/2 text-muted" />
          <Input
            className="pl-9"
            placeholder="Szukaj po nazwie"
            value={query.search ?? ''}
            onChange={(event) => setQuery({ ...query, page: 1, search: event.target.value })}
          />
        </div>
        <Select
          className="sm:w-48"
          value={query.type}
          onChange={(event) => setQuery({ ...query, page: 1, type: event.target.value as WorkspaceType | '' })}
        >
          <option value="">Wszystkie typy</option>
          {WORKSPACE_TYPES.map((type) => (
            <option key={type} value={type}>
              {typeMeta[type].label}
            </option>
          ))}
        </Select>
        <Input
          type="number"
          min={1}
          className="sm:w-36"
          placeholder="Min. miejsc"
          value={query.minCapacity === '' ? '' : query.minCapacity}
          onChange={(event) =>
            setQuery({ ...query, page: 1, minCapacity: event.target.value === '' ? '' : Number(event.target.value) })
          }
        />
      </Card>

      {loading ? (
        <PageLoader />
      ) : data && data.items.length > 0 ? (
        <>
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {data.items.map((workspace) => {
              const meta = typeMeta[workspace.type]
              const Icon = meta.icon
              return (
                <Card key={workspace.id} className={cn('flex flex-col border-l-4 p-5', meta.accent)}>
                  <div className="flex items-start justify-between">
                    <div className={cn('flex size-10 items-center justify-center rounded-lg', meta.iconClass)}>
                      <Icon size={20} />
                    </div>
                    <Badge tone={workspace.isActive ? 'success' : 'neutral'}>
                      {workspace.isActive ? 'Aktywna' : 'Nieaktywna'}
                    </Badge>
                  </div>
                  <h3 className="mt-4 font-semibold text-fg">{workspace.name}</h3>
                  <p className="text-sm text-muted">{meta.label}</p>
                  <p className="mt-2 flex items-start gap-1.5 text-xs text-muted">
                    <MapPin size={14} className="mt-0.5 shrink-0" />
                    {locationLine(workspace)}
                  </p>
                  <div className="mt-4 flex items-center justify-between text-sm">
                    <span className="flex items-center gap-1.5 text-muted">
                      <Users size={15} />
                      {workspace.capacity}
                    </span>
                    <span className="font-medium text-fg">{workspace.pricePerHour.toFixed(2)} / h</span>
                  </div>
                  <div className="mt-5 flex gap-2 border-t border-border pt-4">
                    {!isAdmin && workspace.isActive && (
                      <Button
                        size="sm"
                        className="flex-1"
                        onClick={() => {
                          setReserveTarget(workspace)
                          setReserveStart('')
                          setReserveEnd('')
                        }}
                      >
                        <CalendarPlus size={15} />
                        Rezerwuj
                      </Button>
                    )}
                    {isAdmin && (
                      <>
                        <Button size="sm" variant="secondary" className="flex-1" onClick={() => openEdit(workspace)}>
                          <Pencil size={15} />
                          Edytuj
                        </Button>
                        <Button size="sm" variant="danger" onClick={() => setDeleteTarget(workspace)} aria-label="Usuń">
                          <Trash2 size={15} />
                        </Button>
                      </>
                    )}
                  </div>
                </Card>
              )
            })}
          </div>

          <div className="flex items-center justify-between">
            <p className="text-sm text-muted">
              Strona {data.page} z {Math.max(1, data.totalPages)} · {data.totalCount} wyników
            </p>
            <div className="flex gap-2">
              <Button
                size="sm"
                variant="secondary"
                disabled={data.page <= 1}
                onClick={() => setQuery({ ...query, page: data.page - 1 })}
              >
                Poprzednia
              </Button>
              <Button
                size="sm"
                variant="secondary"
                disabled={data.page >= data.totalPages}
                onClick={() => setQuery({ ...query, page: data.page + 1 })}
              >
                Następna
              </Button>
            </div>
          </div>
        </>
      ) : (
        <Card>
          <EmptyState
            icon={<LayoutGrid size={22} />}
            title="Brak przestrzeni"
            description="Żadna przestrzeń nie pasuje do wybranych filtrów."
          />
        </Card>
      )}

      <Modal
        open={formModal !== null}
        onClose={() => setFormModal(null)}
        title={formModal?.workspace ? 'Edytuj przestrzeń' : 'Nowa przestrzeń'}
      >
        <form onSubmit={handleSave} className="flex flex-col gap-4">
          <Field label="Nazwa">
            <Input value={form.name} onChange={(event) => setForm({ ...form, name: event.target.value })} required />
          </Field>
          <div className="grid grid-cols-2 gap-4">
            <Field label="Typ">
              <Select value={form.type} onChange={(event) => setForm({ ...form, type: event.target.value as WorkspaceType })}>
                {WORKSPACE_TYPES.map((type) => (
                  <option key={type} value={type}>
                    {typeMeta[type].label}
                  </option>
                ))}
              </Select>
            </Field>
            <Field label="Liczba miejsc">
              <Input
                type="number"
                min={1}
                value={form.capacity}
                onChange={(event) => setForm({ ...form, capacity: Number(event.target.value) })}
                required
              />
            </Field>
          </div>
          <div className="grid grid-cols-2 gap-4">
            <Field label="Cena za godzinę">
              <Input
                type="number"
                min={0}
                step={0.5}
                value={form.pricePerHour}
                onChange={(event) => setForm({ ...form, pricePerHour: Number(event.target.value) })}
                required
              />
            </Field>
            <Field label="Piętro">
              <Input
                type="number"
                value={form.floor}
                onChange={(event) => setForm({ ...form, floor: Number(event.target.value) })}
                required
              />
            </Field>
          </div>
          <Field label="Miasto">
            <Input value={form.city} onChange={(event) => setForm({ ...form, city: event.target.value })} required />
          </Field>
          <div className="grid grid-cols-[1fr_120px] gap-4">
            <Field label="Ulica">
              <Input value={form.street} onChange={(event) => setForm({ ...form, street: event.target.value })} required />
            </Field>
            <Field label="Nr budynku">
              <Input
                value={form.buildingNumber}
                onChange={(event) => setForm({ ...form, buildingNumber: event.target.value })}
                required
              />
            </Field>
          </div>
          <div className="pt-1">
            <Switch
              checked={form.isActive ?? true}
              onChange={(checked) => setForm({ ...form, isActive: checked })}
              label="Aktywna"
            />
          </div>
          <div className="mt-2 flex justify-end gap-2">
            <Button type="button" variant="secondary" onClick={() => setFormModal(null)}>
              Anuluj
            </Button>
            <Button type="submit" disabled={saving}>
              {saving ? 'Zapisywanie…' : 'Zapisz'}
            </Button>
          </div>
        </form>
      </Modal>

      <Modal
        open={reserveTarget !== null}
        onClose={() => setReserveTarget(null)}
        title="Rezerwuj przestrzeń"
        description={reserveTarget ? `${reserveTarget.name} · ${locationLine(reserveTarget)}` : undefined}
      >
        <form onSubmit={handleReserve} className="flex flex-col gap-4">
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <Field label="Początek">
              <Input type="datetime-local" value={reserveStart} onChange={(event) => setReserveStart(event.target.value)} required />
            </Field>
            <Field label="Koniec">
              <Input type="datetime-local" value={reserveEnd} onChange={(event) => setReserveEnd(event.target.value)} required />
            </Field>
          </div>
          <div className="flex items-center justify-between rounded-lg bg-surface-2 px-4 py-3 text-sm">
            <span className="text-muted">Szacowana cena</span>
            <span className="font-semibold text-fg">{price === null ? '—' : `${price.toFixed(2)}`}</span>
          </div>
          <div className="mt-2 flex justify-end gap-2">
            <Button type="button" variant="secondary" onClick={() => setReserveTarget(null)}>
              Anuluj
            </Button>
            <Button type="submit" disabled={reserving}>
              {reserving ? 'Rezerwowanie…' : 'Potwierdź rezerwację'}
            </Button>
          </div>
        </form>
      </Modal>

      <Modal
        open={deleteTarget !== null}
        onClose={() => setDeleteTarget(null)}
        title="Usunąć przestrzeń?"
        description={deleteTarget?.name}
      >
        <p className="text-sm text-muted">Tej operacji nie można cofnąć.</p>
        <div className="mt-4 flex justify-end gap-2">
          <Button variant="secondary" onClick={() => setDeleteTarget(null)}>
            Anuluj
          </Button>
          <Button variant="danger" onClick={handleDelete}>
            Usuń
          </Button>
        </div>
      </Modal>
    </div>
  )
}
