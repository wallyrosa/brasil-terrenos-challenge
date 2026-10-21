export const BRAZIL_TIME_ZONE = 'America/Sao_Paulo'

const BRAZIL_UTC_OFFSET = '-03:00'
const DATE_TIME_PATTERN = /^(\d{2})\/(\d{2})\/(\d{4}) (\d{2}):(\d{2})$/

export function maskDateTime(value: string): string {
  const digits = value.replace(/\D/g, '').slice(0, 12)
  const parts = [
    digits.slice(0, 2),
    digits.slice(2, 4),
    digits.slice(4, 8),
    digits.slice(8, 10),
    digits.slice(10, 12),
  ]
  const separators = ['', '/', '/', ' ', ':']

  return parts
    .map((part, index) => (part ? separators[index] + part : ''))
    .join('')
}

const brazilianPartsFormatter = new Intl.DateTimeFormat('en-CA', {
  timeZone: BRAZIL_TIME_ZONE,
  year: 'numeric',
  month: '2-digit',
  day: '2-digit',
  hour: '2-digit',
  minute: '2-digit',
  hourCycle: 'h23',
})

export function nextMinute(): Date {
  const date = new Date()
  date.setSeconds(0, 0)
  date.setMinutes(date.getMinutes() + 1)
  return date
}

export function toBrazilianPickerValue(date: Date): string {
  const parts = Object.fromEntries(
    brazilianPartsFormatter.formatToParts(date).map((part) => [part.type, part.value]),
  )
  return `${parts.year}-${parts.month}-${parts.day}T${parts.hour}:${parts.minute}`
}

export function isComplete(value: string): boolean {
  return DATE_TIME_PATTERN.test(value)
}

export function toPickerValue(value: string): string {
  const match = DATE_TIME_PATTERN.exec(value)
  if (!match) return ''

  const [, day, month, year, hour, minute] = match
  return `${year}-${month}-${day}T${hour}:${minute}`
}

export function fromPickerValue(value: string): string {
  const [date, time] = value.split('T')
  if (!date || !time) return ''

  const [year, month, day] = date.split('-')
  return `${day}/${month}/${year} ${time.slice(0, 5)}`
}

export function parseBrazilianDateTime(value: string): Date | null {
  const match = DATE_TIME_PATTERN.exec(value)
  if (!match) return null

  const [, day, month, year, hour, minute] = match
  if (Number(hour) > 23 || Number(minute) > 59) return null

  const calendarDate = new Date(Date.UTC(Number(year), Number(month) - 1, Number(day)))
  const isRealDate =
    calendarDate.getUTCFullYear() === Number(year) &&
    calendarDate.getUTCMonth() === Number(month) - 1 &&
    calendarDate.getUTCDate() === Number(day)
  if (!isRealDate) return null

  return new Date(`${year}-${month}-${day}T${hour}:${minute}:00${BRAZIL_UTC_OFFSET}`)
}
