import { useCallback, useEffect, useState } from 'react'
import { ErrorBanner } from '../components/ErrorBanner'
import { cancelReservation, getReservations, payReservation } from '../api/reservations'
import type { PaymentMethod, Reservation } from '../types/models'

const PAYMENT_METHODS: PaymentMethod[] = ['Card', 'BankTransfer', 'Cash']

function formatDate(value: string): string {
  return new Date(value).toLocaleString()
}

export function ReservationsPage() {
  const [reservations, setReservations] = useState<Reservation[]>([])
  const [method, setMethod] = useState<PaymentMethod>('Card')
  const [error, setError] = useState<unknown>(null)
  const [notice, setNotice] = useState<string | null>(null)

  const load = useCallback(async () => {
    setError(null)
    try {
      setReservations(await getReservations())
    } catch (loadError) {
      setError(loadError)
    }
  }, [])

  useEffect(() => {
    void load()
  }, [load])

  async function handlePay(id: string) {
    setError(null)
    setNotice(null)
    try {
      await payReservation(id, method)
      setNotice('Payment successful. A confirmation email has been queued.')
      await load()
    } catch (payError) {
      setError(payError)
    }
  }

  async function handleCancel(id: string) {
    setError(null)
    setNotice(null)
    try {
      await cancelReservation(id)
      setNotice('Reservation cancelled.')
      await load()
    } catch (cancelError) {
      setError(cancelError)
    }
  }

  return (
    <div>
      <h1>My reservations</h1>
      <ErrorBanner error={error} />
      {notice && <div className="banner banner-success">{notice}</div>}

      <div className="toolbar">
        <label>
          Payment method
          <select value={method} onChange={(event) => setMethod(event.target.value as PaymentMethod)}>
            {PAYMENT_METHODS.map((option) => (
              <option key={option} value={option}>
                {option}
              </option>
            ))}
          </select>
        </label>
      </div>

      <table className="table">
        <thead>
          <tr>
            <th>Workspace</th>
            <th>Start</th>
            <th>End</th>
            <th>Price</th>
            <th>Status</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          {reservations.map((reservation) => (
            <tr key={reservation.id}>
              <td>{reservation.workspaceName}</td>
              <td>{formatDate(reservation.startTime)}</td>
              <td>{formatDate(reservation.endTime)}</td>
              <td>{reservation.totalPrice.toFixed(2)}</td>
              <td>
                <span className={`badge badge-${reservation.status.toLowerCase()}`}>{reservation.status}</span>
              </td>
              <td className="row-actions">
                {reservation.status === 'Pending' && (
                  <button type="button" onClick={() => handlePay(reservation.id)}>
                    Pay
                  </button>
                )}
                {reservation.status !== 'Cancelled' && (
                  <button type="button" className="danger" onClick={() => handleCancel(reservation.id)}>
                    Cancel
                  </button>
                )}
              </td>
            </tr>
          ))}
          {reservations.length === 0 && (
            <tr>
              <td colSpan={6}>You have no reservations yet.</td>
            </tr>
          )}
        </tbody>
      </table>
    </div>
  )
}
