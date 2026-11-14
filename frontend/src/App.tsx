import { Navigate, Route, Routes } from 'react-router-dom'
import { Layout } from './components/Layout'
import { ProtectedRoute } from './components/ProtectedRoute'
import { IncidentsPage } from './pages/IncidentsPage'
import { LoginPage } from './pages/LoginPage'
import { RegisterPage } from './pages/RegisterPage'
import { ReservationsPage } from './pages/ReservationsPage'
import { WorkspacesPage } from './pages/WorkspacesPage'

export default function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/register" element={<RegisterPage />} />
      <Route
        element={
          <ProtectedRoute>
            <Layout />
          </ProtectedRoute>
        }
      >
        <Route path="/workspaces" element={<WorkspacesPage />} />
        <Route path="/reservations" element={<ReservationsPage />} />
        <Route path="/incidents" element={<IncidentsPage />} />
        <Route path="/" element={<Navigate to="/workspaces" replace />} />
      </Route>
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  )
}
