import { BRAZIL_TIME_ZONE } from './dateTime'

const currencyFormatter =new Intl.NumberFormat('pt-BR', {
  style: 'currency',
  currency: 'BRL',
})

const dateTimeFormatter = new Intl.DateTimeFormat('pt-BR', {
  dateStyle: 'short',
  timeStyle: 'short',
  hourCycle: 'h23',
  timeZone: BRAZIL_TIME_ZONE,
})

export function formatCurrency(value: number): string {
  return currencyFormatter.format(value)
}

export function formatDateTime(iso: string | null | undefined): string {
  if (!iso) return '-'
  return dateTimeFormatter.format(new Date(iso))
}

export function formatDocument(document: string): string {
  const digits = document.replace(/\D/g, '')
  if (digits.length === 11) {
    return digits.replace(/(\d{3})(\d{3})(\d{3})(\d{2})/, '$1.$2.$3-$4')
  }
  if (digits.length === 14) {
    return digits.replace(/(\d{2})(\d{3})(\d{3})(\d{4})(\d{2})/, '$1.$2.$3/$4-$5')
  }
  return document
}

export function shortId(id: string): string {
  return id.slice(0, 8)
}
