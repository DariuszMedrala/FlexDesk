import type { ReactNode } from 'react'
import { CalendarCheck, LayoutGrid, Mail, Moon, ShieldCheck, Sun } from 'lucide-react'
import { useTheme } from '../theme/ThemeContext'

const highlights = [
  { icon: LayoutGrid, title: 'Rezerwuj w kilka sekund', text: 'Biurka, sale konferencyjne i prywatne biura w jednym miejscu.' },
  { icon: CalendarCheck, title: 'Pełna kontrola', text: 'Opłacaj, przeglądaj i anuluj rezerwacje bez dzwonienia do recepcji.' },
  { icon: ShieldCheck, title: 'Bezpieczny dostęp', text: 'Uwierzytelnianie JWT i uprawnienia oparte na rolach.' },
  { icon: Mail, title: 'Potwierdzenia e-mail', text: 'Automatyczne powiadomienia po każdej opłaconej rezerwacji.' },
]

export function AuthLayout({ children }: { children: ReactNode }) {
  const { theme, toggle } = useTheme()

  return (
    <div className="relative flex min-h-screen bg-bg">
      <button
        type="button"
        onClick={toggle}
        aria-label="Przełącz motyw"
        className="absolute top-5 right-5 z-20 flex size-11 items-center justify-center rounded-xl border border-border bg-surface text-muted shadow-card transition-colors hover:bg-surface-2 hover:text-fg"
      >
        {theme === 'dark' ? <Sun size={20} /> : <Moon size={20} />}
      </button>

      <div className="relative hidden w-1/2 flex-col justify-center overflow-hidden bg-gradient-to-br from-[#5e6ad2] via-[#6a62d6] to-[#8b5cf6] p-14 text-white lg:flex">
        <div className="pointer-events-none absolute -top-24 -right-24 size-80 rounded-full bg-white/10 blur-3xl" />
        <div className="pointer-events-none absolute -bottom-32 -left-20 size-96 rounded-full bg-black/10 blur-3xl" />

        <div className="relative z-10 mx-auto flex w-full max-w-md flex-col gap-12">
          <div className="flex flex-col items-center gap-4 text-center">
            <div className="flex size-20 items-center justify-center rounded-3xl bg-white/15 shadow-lg ring-1 ring-white/20">
              <LayoutGrid size={40} />
            </div>
            <div>
              <h1 className="text-4xl font-bold tracking-tight">FlexDesk</h1>
              <p className="mt-2 text-lg text-white/80">System rezerwacji przestrzeni co-workingowej</p>
            </div>
          </div>

          <ul className="flex flex-col gap-5">
            {highlights.map(({ icon: Icon, title, text }) => (
              <li key={title} className="flex items-start gap-4">
                <span className="flex size-11 shrink-0 items-center justify-center rounded-xl bg-white/15 ring-1 ring-white/10">
                  <Icon size={20} />
                </span>
                <div>
                  <p className="font-semibold">{title}</p>
                  <p className="text-sm text-white/75">{text}</p>
                </div>
              </li>
            ))}
          </ul>
        </div>
      </div>

      <div className="flex w-full items-center justify-center px-6 py-16 lg:w-1/2">
        <div className="w-full max-w-md">
          <div className="mb-10 flex flex-col items-center gap-3 text-center lg:hidden">
            <div className="flex size-14 items-center justify-center rounded-2xl bg-primary text-primary-fg">
              <LayoutGrid size={28} />
            </div>
            <span className="text-2xl font-bold tracking-tight text-fg">FlexDesk</span>
          </div>
          {children}
        </div>
      </div>
    </div>
  )
}
