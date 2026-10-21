import { useRef } from 'react'
import {
  fromPickerValue,
  maskDateTime,
  nextMinute,
  toBrazilianPickerValue,
  toPickerValue,
} from '@/shared/lib/dateTime'
import { Input } from './Field'

interface DateTimeInputProps {
  value: string
  onChange: (value: string) => void
}

function clampToMin(value: string, min: string): string {
  if (!value) return value
  return value < min ? min : value
}

export function DateTimeInput({ value, onChange }: DateTimeInputProps) {
  const pickerRef = useRef<HTMLInputElement>(null)
  const minPickerValue = toBrazilianPickerValue(nextMinute())

  const openPicker = () => {
    const picker = pickerRef.current
    if (!picker) return

    if (typeof picker.showPicker === 'function') {
      picker.showPicker()
    } else {
      picker.focus()
      picker.click()
    }
  }

  return (
    <div className="date-time-input">
      <Input
        type="text"
        inputMode="numeric"
        autoComplete="off"
        placeholder="dd/mm/aaaa hh:mm"
        maxLength={16}
        value={value}
        onChange={(event) => onChange(maskDateTime(event.target.value))}
      />
      <button
        type="button"
        className="date-time-input__button"
        aria-label="Abrir calendário"
        title="Abrir calendário"
        onClick={openPicker}
      >
        <svg viewBox="0 0 24 24" width="18" height="18" aria-hidden="true">
          <rect x="3" y="5" width="18" height="16" rx="2" fill="none" stroke="currentColor" strokeWidth="2" />
          <path d="M3 10h18M8 3v4M16 3v4" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" />
        </svg>
      </button>
      <input
        ref={pickerRef}
        type="datetime-local"
        className="date-time-input__picker"
        tabIndex={-1}
        aria-hidden="true"
        min={minPickerValue}
        value={toPickerValue(value)}
        onChange={(event) => onChange(fromPickerValue(clampToMin(event.target.value, minPickerValue)))}
      />
    </div>
  )
}
