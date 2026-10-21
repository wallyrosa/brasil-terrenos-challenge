import type { ReactNode } from 'react'

type AlertTone = 'success' | 'danger' | 'info' | 'warning'

interface AlertProps {
  tone?: AlertTone
  title?: string
  children?: ReactNode
}

export function Alert({ tone = 'info', title, children }: AlertProps) {
  return (
    <div className={`alert alert--${tone}`} role={tone === 'danger' ? 'alert' : 'status'}>
      {title && <strong className="alert__title">{title}</strong>}
      {children && <div>{children}</div>}
    </div>
  )
}
