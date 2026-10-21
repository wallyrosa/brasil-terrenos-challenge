import type { ReactNode } from 'react'

export interface DataListItem {
  label: string
  value: ReactNode
}

interface DataListProps {
  items: DataListItem[]
}

export function DataList({ items }: DataListProps) {
  return (
    <dl className="data-list">
      {items.map((item) => (
        <div key={item.label} className="data-list__item">
          <dt>{item.label}</dt>
          <dd>{item.value}</dd>
        </div>
      ))}
    </dl>
  )
}
