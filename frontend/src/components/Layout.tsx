import { NavLink, Outlet, useNavigate } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext'

export function Layout() {
  const { user, isAdmin, logout } = useAuth()
  const navigate = useNavigate()

  function handleLogout() {
    logout()
    navigate('/login')
  }

  return (
    <div className="app">
      <header className="nav">
        <div className="brand">FlexDesk</div>
        <nav className="nav-links">
          <NavLink to="/workspaces">Workspaces</NavLink>
          <NavLink to="/reservations">Reservations</NavLink>
          <NavLink to="/incidents">Incidents</NavLink>
        </nav>
        <div className="nav-user">
          <span>
            {user?.email}
            {isAdmin ? ' · admin' : ''}
          </span>
          <button type="button" onClick={handleLogout}>
            Log out
          </button>
        </div>
      </header>
      <main className="content">
        <Outlet />
      </main>
    </div>
  )
}
