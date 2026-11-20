import { useCallback, useEffect, useMemo, useState } from 'react'
import { CalendarCheck } from 'lucide-react'
import { useAuth } from '../auth/AuthContext'
import { cancelReservation, getReservations, payReservation } from '../api/reservations'
import { Badge } from '../components/ui/Badge'
import { Button } from '../components/ui/Button'
import { Card } from '../components/ui/Card'
import { EmptyState } from '../components/ui/EmptyState'
import { Field } from '../components/ui/Field'
import { Modal } from '../components/ui/Modal'
import { Select } from '../components/ui/Select'
import { PageLoader } from '../components/ui/Spinner'
import { cn } from '../lib/cn'
import { getErrorMessage } from '../lib/errors'
import { useToast } from '../toast/ToastContext'
import type { PaymentMethod, Reservation, ReservationStatus } from '../types/models'

const PAYMENT_METHODS: PaymentMethod[] = ['Card', 'BankTransfer', 'Cash']

const statusTone: Record<ReservationStatus, 'warning' | 'success' | 'neutral' | 'info'> = {
  Pending: 'warning',
  Confirmed: 'success',
  Cancelled: 'neutral',
  Completed: 'info',
}

const statusLabel: Record<ReservationStatus, string> = {
  Pending: 'Oczekująca',
  Confirmed: 'Potwierdzona',
  Cancelled: 'Anulowana',
  Completed: 'Odbyta',
}

type StatusFilter = ReservationStatus | 'All'

const statusFilters: { value: StatusFilter; label: string }[] = [
  { value: 'All', label: 'Wszystkie' },
  { value: 'Pending', label: 'Oczekujące' },
  { value: 'Confirmed', label: 'Potwierdzone' },
  { value: 'Completed', label: 'Odbyte' },
  { value: 'Cancelled', label: 'Anulowane' },
]

const PAGE_SIZE = 8

function formatDate(value: string): string {
  return new Date(value).toLocaleString()
}

function sortByUpcoming(list: Reservation[]): Reservation[] {
  const startOfToday = new Date()
  startOfToday.setHours(0, 0, 0, 0)
  const todayMs = startOfToday.getTime()

  return [...list].sort((a, b) => {
    const aStart = new Date(a.startTime).getTime()
    const bStart = new Date(b.startTime).getTime()
    const aUpcoming = aStart >= todayMs
    const bUpcoming = bStart >= todayMs
    if (aUpcoming !== bUpcoming) {
      return aUpcoming ? -1 : 1
    }
    return aStart - bStart
  })
}

export function ReservationsPage() {
  const { isAdmin } = useAuth()
  const toast = useToast()
  const [reservations, setReservations] = useState<Reservation[]>([])
  const [loading, setLoading] = useState(true)
  const [filter, setFilter] = useState<StatusFilter>('All')
  const [page, setPage] = useState(1)

  const [payTarget, setPayTarget] = useState<Reservation | null>(null)
  const [method, setMethod] = useState<PaymentMethod>('Card')
  const [paying, setPaying] = useState(false)

  const [cancelTarget, setCancelTarget] = useState<Reservation | null>(null)

  const load = useCallback(async () => {
    setLoading(true)
    try {
      setReservations(await getReservations())
    } catch (error) {
      toast.error(getErrorMessage(error))
    } finally {
      setLoading(false)
    }
  }, [toast])

  useEffect(() => {
    void load()
  }, [load])

  const visible = useMemo(() => {
    const filtered = filter === 'All' ? reservations : reservations.filter((item) => item.status === filter)
    return sortByUpcoming(filtered)
  }, [reservations, filter])

  const totalPages = Math.max(1, Math.ceil(visible.length / PAGE_SIZE))
  const safePage = Math.min(page, totalPages)
  const pageItems = visible.slice((safePage - 1) * PAGE_SIZE, safePage * PAGE_SIZE)

  async function handlePay() {
    if (!payTarget) {
      return
    }
    setPaying(true)
    try {
      await payReservation(payTarget.id, method)
      toast.success('Płatność zrealizowana. E-mail z potwierdzeniem został wysłany.')
      setPayTarget(null)
      await load()
    } catch (error) {
      toast.error(getErrorMessage(error))
    } finally {
      setPaying(false)
    }
  }

  async function handleCancel() {
    if (!cancelTarget) {
      return
    }
    try {
      await cancelReservation(cancelTarget.id)
      toast.success('Rezerwacja anulowana.')
      setCancelTarget(null)
      await load()
    } catch (error) {
      toast.error(getErrorMessage(error))
    }
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold text-fg">{isAdmin ? 'Wszystkie rezerwacje' : 'Moje rezerwacje'}</h1>
        <p className="mt-1 text-sm text-muted">
          {isAdmin
            ? 'Rezerwacje wszystkich użytkowników.'
            : 'Opłacaj i zarządzaj swoimi rezerwacjami. Nieopłacona rezerwacja zostaje automatycznie anulowana na godzinę przed rozpoczęciem.'}
        </p>
      </div>

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
          <EmptyState
            icon={<CalendarCheck size={22} />}
            title="Brak rezerwacji"
            description={isAdmin ? 'Brak rezerwacji dla wybranego filtra.' : 'Zarezerwuj przestrzeń w zakładce Przestrzenie.'}
          />
        </Card>
      ) : (
        <>
          <Card className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-border text-left text-xs font-medium text-muted">
                <th className="px-5 py-3">Przestrzeń</th>
                {isAdmin && <th className="px-5 py-3">Użytkownik</th>}
                <th className="px-5 py-3">Początek</th>
                <th className="px-5 py-3">Koniec</th>
                <th className="px-5 py-3">Cena</th>
                <th className="px-5 py-3">Status</th>
                <th className="px-5 py-3 text-right">Akcje</th>
              </tr>
            </thead>
            <tbody>
              {pageItems.map((reservation) => (
                <tr key={reservation.id} className="border-b border-border last:border-0">
                  <td className="px-5 py-3 font-medium text-fg">{reservation.workspaceName}</td>
                  {isAdmin && <td className="px-5 py-3 text-muted">{reservation.userFullName}</td>}
                  <td className="px-5 py-3 text-muted">{formatDate(reservation.startTime)}</td>
                  <td className="px-5 py-3 text-muted">{formatDate(reservation.endTime)}</td>
                  <td className="px-5 py-3 text-fg">{reservation.totalPrice.toFixed(2)}</td>
                  <td className="px-5 py-3">
                    <Badge tone={statusTone[reservation.status]}>{statusLabel[reservation.status]}</Badge>
                  </td>
                  <td className="px-5 py-3">
                    <div className="flex justify-end gap-2">
                      {!isAdmin && reservation.status === 'Pending' && (
                        <Button
                          size="sm"
                          onClick={() => {
                            setPayTarget(reservation)
                            setMethod('Card')
                          }}
                        >
                          Opłać
                        </Button>
                      )}
                      {(reservation.status === 'Pending' || reservation.status === 'Confirmed') && (
                        <Button size="sm" variant="danger" onClick={() => setCancelTarget(reservation)}>
                          Anuluj
                        </Button>
                      )}
                    </div>
                  </td>
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

      <Modal
        open={payTarget !== null}
        onClose={() => setPayTarget(null)}
        title="Opłać rezerwację"
        description={payTarget ? `${payTarget.workspaceName} · ${payTarget.totalPrice.toFixed(2)}` : undefined}
      >
        <div className="flex flex-col gap-4">
          <Field label="Metoda płatności">
            <Select value={method} onChange={(event) => setMethod(event.target.value as PaymentMethod)}>
              {PAYMENT_METHODS.map((option) => (
                <option key={option} value={option}>
                  {option}
                </option>
              ))}
            </Select>
          </Field>
          <div className="flex justify-end gap-2">
            <Button variant="secondary" onClick={() => setPayTarget(null)}>
              Anuluj
            </Button>
            <Button onClick={handlePay} disabled={paying}>
              {paying ? 'Przetwarzanie…' : 'Zapłać'}
            </Button>
          </div>
        </div>
      </Modal>

      <Modal
        open={cancelTarget !== null}
        onClose={() => setCancelTarget(null)}
        title="Anulować rezerwację?"
        description={cancelTarget?.workspaceName}
      >
        <p className="text-sm text-muted">Rezerwację można anulować najpóźniej 24 godziny przed jej rozpoczęciem.</p>
        <div className="mt-4 flex justify-end gap-2">
          <Button variant="secondary" onClick={() => setCancelTarget(null)}>
            Wróć
          </Button>
          <Button variant="danger" onClick={handleCancel}>
            Anuluj rezerwację
          </Button>
        </div>
      </Modal>
    </div>
  )
}
