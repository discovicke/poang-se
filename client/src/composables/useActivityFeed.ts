import { reactive } from 'vue'

export interface ActivityEvent {
  id: number
  icon: string
  message: string
  timestamp: Date
  read: boolean
}

/** Module-level singleton — shared across all component instances. */
const state = reactive({
  events: [] as ActivityEvent[],
  unreadCount: 0,
})

let nextId = 1

export function useActivityFeed() {
  function push(icon: string, message: string) {
    state.events.unshift({
      id: nextId++,
      icon,
      message,
      timestamp: new Date(),
      read: false,
    })
    state.unreadCount++
    if (state.events.length > 30) state.events.pop()
  }

  function markAllRead() {
    state.events.forEach(e => (e.read = true))
    state.unreadCount = 0
  }

  function clear() {
    state.events.splice(0)
    state.unreadCount = 0
  }

  /** "2 min sedan" / "just nu" etc */
  function timeAgo(date: Date): string {
    const secs = Math.floor((Date.now() - date.getTime()) / 1000)
    if (secs < 10) return 'just nu'
    if (secs < 60) return `${secs}s sedan`
    const mins = Math.floor(secs / 60)
    if (mins < 60) return `${mins} min sedan`
    return `${Math.floor(mins / 60)} h sedan`
  }

  return { state, push, markAllRead, clear, timeAgo }
}

