export function formatCount(count: number, singular: string, plural: string): string {
  // Posebni slučajevi: 11, 12, 13, 14 - uvijek množina
  if (count >= 11 && count <= 14) {
    return `${count} ${plural}`
  }
  
  // Brojevi koji završavaju na 1 (osim 11): 1, 21, 31, 41, 51, 61, 71, 81, 91, 101, 121, 131, itd.
  const lastDigit = count % 10
  if (lastDigit === 1 && count !== 11) {
    return `${count} ${singular}`
  }
  
  // Svi ostali brojevi: množina
  return `${count} ${plural}`
}

// Česti primjeri za brže korištenje
export const commonCounts = {
  video: (count: number) => formatCount(count, 'video', 'videa'),
  korisnik: (count: number) => formatCount(count, 'korisnik', 'korisnika'),
  datoteka: (count: number) => formatCount(count, 'datoteka', 'datoteke'),
  poruka: (count: number) => formatCount(count, 'poruka', 'poruke'),
  komentar: (count: number) => formatCount(count, 'komentar', 'komentara'),
  pretplata: (count: number) => formatCount(count, 'pretplata', 'pretplate'),
  uplata: (count: number) => formatCount(count, 'uplata', 'uplate'),
  greška: (count: number) => formatCount(count, 'greška', 'greške'),
  sekunda: (count: number) => formatCount(count, 'sekunda', 'sekundi'),
  minuta: (count: number) => formatCount(count, 'minuta', 'minuta'),
  sat: (count: number) => formatCount(count, 'sat', 'sati'),
  dan: (count: number) => formatCount(count, 'dan', 'dana'),
  tjedan: (count: number) => formatCount(count, 'tjedan', 'tjedna'),
  mjesec: (count: number) => formatCount(count, 'mjesec', 'mjeseci'),
  godina: (count: number) => formatCount(count, 'godina', 'godina')
} 