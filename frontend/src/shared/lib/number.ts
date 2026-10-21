export function parseDecimal(value: string): number {
  return Number(value.replace(',', '.'))
}

export function isPositiveNumber(value: number): boolean {
  return Number.isFinite(value) && value > 0
}

export function isNonNegativeNumber(value: number): boolean {
  return Number.isFinite(value) && value >= 0
}
