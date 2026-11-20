import { CalendarCheck, LayoutGrid, LogOut, Moon, Settings, Sun, TriangleAlert, Users } from 'lucide-react'
import type { LucideIcon } from 'lucide-react'
import { NavLink, Outlet, useNavigate } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext'
import { cn } from '../lib/cn'
import { useTheme } from '../theme/ThemeContext'

interface NavItem {
  to: string
  label: string
  icon: LucideIcon
}

const navItems: NavItem[] = [
  { to: '/workspaces', label: 'Przestrzenie', icon: LayoutGrid },
  { to: '/reservations', label: 'Rezerwacje', icon: CalendarCheck },
  { to: '/incidents', label: 'Zgłoszenia', icon: TriangleAlert },
]

const adminNavItems: NavItem[] = [{ to: '/users', label: 'Użytkownicy', icon: Users }]

const settingsNavItem: NavItem = { to: '/settings', label: 'Ustawienia', icon: Settings }

function BrandMark() {
  return (
    <div className="flex items-center gap-2 px-2">
      <div className="flex size-8 items-center justify-center rounded-lg bg-primary text-primary-fg">
        <LayoutGrid size={18} />
      </div>
      <span className="text-base font-semibold tracking-tight text-fg">FlexDesk</span>
    </div>
  )
}

function NavItems({ items }: { items: NavItem[] }) {
  return (
    <>
      {items.map(({ to, label, icon: Icon }) => (
        <NavLink
          key={to}
          to={to}
          className={({ isActive }) =>
            cn(
              'flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-colors',
              isActive ? 'bg-surface-2 text-fg' : 'text-muted hover:bg-surface-2 hover:text-fg',
            )
          }
        >
          <Icon size={18} />
          {label}
        </NavLink>
      ))}
    </>
  )
}

export function Layout() {
  const { user, isAdmin, logout } = useAuth()
  const { theme, toggle } = useTheme()
  const navigate = useNavigate()

  function handleLogout() {
    logout()
    navigate('/login')
  }

  const navLinks = [...navItems, ...(isAdmin ? adminNavItems : []), settingsNavItem]

  return (
    <div className="flex min-h-screen">
      <aside className="hidden w-60 shrink-0 flex-col border-r border-border bg-surface md:flex">
        <div className="flex h-16 items-center border-b border-border px-4">
          <BrandMark />
        </div>
        <nav className="flex flex-1 flex-col gap-1 p-3">
          <NavItems items={navLinks} />
        </nav>
      </aside>

      <div className="flex min-w-0 flex-1 flex-col">
        <header className="sticky top-0 z-30 flex h-16 items-center gap-4 border-b border-border bg-surface/80 px-4 backdrop-blur md:px-6">
          <div className="md:hidden">
            <BrandMark />
          </div>
          <div className="flex-1" />
          <button
            type="button"
            onClick={toggle}
            aria-label="Toggle theme"
            className="flex size-9 items-center justify-center rounded-lg text-muted transition-colors hover:bg-surface-2 hover:text-fg"
          >
            {theme === 'dark' ? <Sun size={18} /> : <Moon size={18} />}
          </button>
          <div className="flex items-center gap-3">
            <div className="hidden text-right sm:block">
              <p className="text-sm font-medium text-fg">{user?.fullName}</p>
              <p className="text-xs text-muted">{isAdmin ? 'Administrator' : 'Użytkownik'}</p>
            </div>
            <button
              type="button"
              onClick={handleLogout}
              aria-label="Log out"
              className="flex size-9 items-center justify-center rounded-lg text-muted transition-colors hover:bg-surface-2 hover:text-fg"
            >
              <LogOut size={18} />
            </button>
          </div>
        </header>

        <nav className="flex gap-1 border-b border-border bg-surface px-3 py-2 md:hidden">
          <NavItems items={navLinks} />
        </nav>

        <main className="mx-auto w-full max-w-6xl flex-1 px-4 py-6 md:px-6 md:py-8">
          <Outlet />
        </main>
      </div>
    </div>
  )
}
