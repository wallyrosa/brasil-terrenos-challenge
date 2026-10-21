interface SpinnerProps {
  label?: string
}

export function Spinner({ label = 'Carregando...' }: SpinnerProps) {
  return (
    <div className="spinner" role="status">
      <span className="spinner__circle" aria-hidden="true" />
      <span>{label}</span>
    </div>
  )
}
