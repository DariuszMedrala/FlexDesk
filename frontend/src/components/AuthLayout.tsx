import type { ReactNode } from 'react'
import { CalendarCheck, LayoutGrid, ShieldCheck } from 'lucide-react'

const highlights = [
  { icon: LayoutGrid, text: 'Przeglądaj i rezerwuj biurka, sale i gabinety' },
  { icon: CalendarCheck, text: 'Opłacaj rezerwacje i zarządzaj nimi w jednym miejscu' },
  { icon: ShieldCheck, text: 'Bezpieczny dostęp oparty na rolach' },
]

export function AuthLayout({ children }: { children: ReactNode }) {
  return (
    <div className="flex min-h-screen bg-bg">
      <div className="relative hidden w-1/2 flex-col justify-between overflow-hidden bg-gradient-to-br from-[#5e6ad2] via-[#6a62d6] to-[#8b5cf6] p-12 text-white lg:flex">
        <div className="flex items-center gap-2">
          <div className="flex size-9 items-center justify-center rounded-lg bg-white/15">
            <LayoutGrid size={20} />
          </div>
          <span className="text-lg font-semibold">FlexDesk</span>
        </div>

        <div className="space-y-6">
          <h1 className="max-w-md text-3xl font-semibold leading-tight">
            Zarządzaj przestrzenią co-workingową bez chaosu.
          </h1>
          <ul className="space-y-3">
            {highlights.map(({ icon: Icon, text }) => (
              <li key={text} className="flex items-center gap-3 text-sm text-white/90">
                <span className="flex size-8 items-center justify-center rounded-lg bg-white/15">
                  <Icon size={16} />
                </span>
                {text}
              </li>
            ))}
          </ul>
        </div>

        <p className="text-sm text-white/70">System rezerwacji biurek, sal i gabinetów.</p>
      </div>

      <div className="flex w-full items-center justify-center px-6 py-12 lg:w-1/2">
        <div className="w-full max-w-sm">{children}</div>
      </div>
    </div>
  )
}
