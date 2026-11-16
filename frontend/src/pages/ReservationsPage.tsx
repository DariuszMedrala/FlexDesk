import { useCallback, useEffect, useState } from 'react'
import { CalendarCheck } from 'lucide-react'
import { cancelReservation, getReservations, payReservation } from '../api/reservations'
import { Badge } from '../components/ui/Badge'
import { Button } from '../components/ui/Button'
import { Card } from '../components/ui/Card'
import { EmptyState } from '../components/ui/EmptyState'
import { Field } from '../components/ui/Field'
import { Modal } from '../components/ui/Modal'
import { Select } from '../components/ui/Select'
import { PageLoader } from '../components/ui/Spinner'
import { getErrorMessage } from '../lib/errors'
import { useToast } from '../toast/ToastContext'
import type { PaymentMethod, Reservation, ReservationStatus } from '../types/models'

const PAYMENT_METHODS: PaymentMethod[] = ['Card', 'BankTransfer', 'Cash']

const statusTone: Record<ReservationStatus, 'warning' | 'success' | 'neutral'> = {
  Pending: 'warning',
  Confirmed: 'success',
  Cancelled: 'neutral',
}

function formatDate(value: string): string {
  return new Date(value).toLocaleString()
}

export function ReservationsPage() {
  const toast = useToast()
  const [reservations, setReservations] = useState<Reservation[]>([])
  const [loading, setLoading] = useState(true)

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
        <h1 className="text-2xl font-semibold text-fg">Moje rezerwacje</h1>
        <p className="mt-1 text-sm text-muted">Opłacaj i zarządzaj swoimi rezerwacjami.</p>
      </div>

      {loading ? (
        <PageLoader />
      ) : reservations.length === 0 ? (
        <Card>
          <EmptyState
            icon={<CalendarCheck size={22} />}
            title="Brak rezerwacji"
            description="Zarezerwuj przestrzeń w zakładce Workspaces."
          />
        </Card>
      ) : (
        <Card className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-border text-left text-xs font-medium text-muted">
                <th className="px-5 py-3">Przestrzeń</th>
                <th className="px-5 py-3">Początek</th>
                <th className="px-5 py-3">Koniec</th>
                <th className="px-5 py-3">Cena</th>
                <th className="px-5 py-3">Status</th>
                <th className="px-5 py-3 text-right">Akcje</th>
              </tr>
            </thead>
            <tbody>
              {reservations.map((reservation) => (
                <tr key={reservation.id} className="border-b border-border last:border-0">
                  <td className="px-5 py-3 font-medium text-fg">{reservation.workspaceName}</td>
                  <td className="px-5 py-3 text-muted">{formatDate(reservation.startTime)}</td>
                  <td className="px-5 py-3 text-muted">{formatDate(reservation.endTime)}</td>
                  <td className="px-5 py-3 text-fg">{reservation.totalPrice.toFixed(2)}</td>
                  <td className="px-5 py-3">
                    <Badge tone={statusTone[reservation.status]}>{reservation.status}</Badge>
                  </td>
                  <td className="px-5 py-3">
                    <div className="flex justify-end gap-2">
                      {reservation.status === 'Pending' && (
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
                      {reservation.status !== 'Cancelled' && (
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
        <p className="text-sm text-muted">
          Rezerwację można anulować najpóźniej 24 godziny przed jej rozpoczęciem.
        </p>
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
