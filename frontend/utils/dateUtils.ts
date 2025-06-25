export function formatDate(dateString: string | number) {
  if (!dateString) return 'Nepoznato'
  
  let date: Date
  if (typeof dateString === 'number') {
    date = new Date(dateString * 1000)
  } else {
    date = new Date(dateString)
  }
  
  if (isNaN(date.getTime())) return 'Nepoznato'
  
  return new Intl.DateTimeFormat('hr-HR', { 
    day: '2-digit', 
    month: '2-digit', 
    year: 'numeric' 
  }).format(date)
}

export function formatDuration(seconds: number) {
  if (!seconds) return ''
  
  const hours = Math.floor(seconds / 3600)
  const minutes = Math.floor((seconds % 3600) / 60)
  const secs = seconds % 60
  
  if (hours > 0) {
    return `${hours}:${minutes.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`
  }
  return `${minutes}:${secs.toString().padStart(2, '0')}`
} 