import type { InputHTMLAttributes } from 'react'
import { cn } from '../../lib/cn'

const fieldBase =
  'h-10 w-full rounded-lg border border-border bg-surface px-3 text-sm text-fg placeholder:text-muted transition-colors focus-visible:outline-none focus-visible:border-ring focus-visible:ring-2 focus-visible:ring-ring/40'

export function Input({ className, ...props }: InputHTMLAttributes<HTMLInputElement>) {
  return <input className={cn(fieldBase, className)} {...props} />
}

export { fieldBase }
