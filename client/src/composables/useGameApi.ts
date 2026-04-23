import { ref } from 'vue'
import type { Game, ScoreChartData } from '../types/game'

/**
 * Composable som wrappar alla REST API anrop för en specifik match.
 * Varje muterande metod returnerar det uppdaterade spelet så att anroparen
 * kan uppdatera sin reaktiva state i ett enda ställe.
 */
export function useGameApi(gameId: string) {
  const loading = ref(true)
  const isLocked = ref(false)
  const lockedGameName = ref<string | null>(null)

  const base = `/api/games/${gameId}`

  function authHeaders(): HeadersInit {
    const token = localStorage.getItem(`gameToken:${gameId}`)
    return token ? { Authorization: `Bearer ${token}` } : {}
  }

  async function fetchGame(): Promise<Game | null> {
    const res = await fetch(base, { headers: authHeaders() })
    if (res.status === 403) {
      const body = await res.json().catch(() => ({}))
      if (body.isPrivate) {
        isLocked.value = true
        lockedGameName.value = body.name ?? null
      }
      return null
    }
    isLocked.value = false
    if (!res.ok)
      return null

    return await res.json()
  }

  async function unlockGame(password: string): Promise<boolean> {
    const res = await fetch(`${base}/unlock`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ password }),
    })
    if (!res.ok)
      return false
    const { token } = await res.json()
    localStorage.setItem(`gameToken:${gameId}`, token)
    isLocked.value = false
    return true
  }

  async function saveSettings(
    secret: string,
    settings: {
      maxRounds: number | null
      scoreIncrement: number
      lowerIsBetter: boolean
      creatorOnly: boolean
      teamBasedWinner: boolean
      gameMode: string | null
      gameModeValue: number | null
      gameModeTarget: string
      startingScore: number
    },
  ): Promise<Game | null> {
    await fetch(`${base}/settings`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json', 'X-Creator-Secret': secret },
      body: JSON.stringify(settings),
    })
    return fetchGame()
  }

  async function addTeam(name: string): Promise<Game | null> {
    await fetch(`${base}/teams`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ name }),
    })
    return fetchGame()
  }

  async function addPlayer(userName: string, teamId: string | null): Promise<Game | null> {
    await fetch(`${base}/players/new`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ userName, teamId: teamId || null }),
    })
    return fetchGame()
  }

  async function assignTeam(playerId: string, teamId: string | null): Promise<Game | null> {
    await fetch(`${base}/players/team`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ playerId, teamId }),
    })
    return fetchGame()
  }

  async function renamePlayer(playerId: string, name: string): Promise<Game | null> {
    await fetch(`${base}/players/${playerId}/rename`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ name }),
    })
    return fetchGame()
  }

  async function removePlayer(playerId: string): Promise<Game | null> {
    await fetch(`${base}/players/${playerId}`, { method: 'DELETE' })
    return fetchGame()
  }

  async function renameTeam(teamId: string, name: string): Promise<Game | null> {
    await fetch(`${base}/teams/${teamId}/rename`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ name }),
    })
    return fetchGame()
  }

  async function removeTeam(teamId: string): Promise<Game | null> {
    await fetch(`${base}/teams/${teamId}`, { method: 'DELETE' })
    return fetchGame()
  }

  async function addScore(
    playerId: string,
    teamId: string | null,
    round: number,
    value: number,
  ): Promise<Game | null> {
    await fetch(`${base}/scores`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ playerId, teamId, round, value }),
    })
    return fetchGame()
  }

  async function updateScore(
    playerId: string,
    round: number,
    value: number,
  ): Promise<Game | null> {
    await fetch(`${base}/scores`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ playerId, round, value }),
    })
    return fetchGame()
  }

  async function drawScoreChart(params: { gameId: string }): Promise<ScoreChartData | null> {
    const res = await fetch(`/api/games/${params.gameId}/score-chart`)
    return await res.json()
  }

  async function startGame(): Promise<Game | null> {
    await fetch(`${base}/start`, { method: 'PUT' })
    return fetchGame()
  }

  async function pauseGame(): Promise<Game | null> {
    await fetch(`${base}/pause`, { method: 'PUT' })
    return fetchGame()
  }

  async function finishGame(): Promise<Game | null> {
    await fetch(`${base}/finish`, { method: 'PUT' })
    return fetchGame()
  }

  async function advanceRound(): Promise<Game | null> {
    await fetch(`${base}/advance-round`, { method: 'PUT' })
    return fetchGame()
  }

  /** Återställer matchen till Waiting med samma spelare/inställningar. Kräver creator-secret. */
  async function resetGame(): Promise<Game | null> {
    const secret = localStorage.getItem(`creator:${gameId}`)
    if (!secret) return null
    await fetch(`${base}/reset`, {
      method: 'PUT',
      headers: { 'X-Creator-Secret': secret },
    })
    return fetchGame()
  }

  /** Skapar en ny match baserad på denna, med ny URL. Returnerar det nya spelets id + secret. */
  async function rematch(): Promise<{ id: string; creatorSecret: string } | null> {
    const secret = localStorage.getItem(`creator:${gameId}`)
    if (!secret) return null
    const res = await fetch(`${base}/rematch`, {
      method: 'POST',
      headers: { 'X-Creator-Secret': secret },
    })
    if (!res.ok) return null
    const data = await res.json()
    return { id: data.id, creatorSecret: data.creatorSecret }
  }

  return {
    loading,
    isLocked,
    lockedGameName,
    fetchGame,
    unlockGame,
    saveSettings,
    addTeam,
    addPlayer,
    assignTeam,
    renamePlayer,
    removePlayer,
    renameTeam,
    removeTeam,
    addScore,
    updateScore,
    startGame,
    pauseGame,
    finishGame,
    advanceRound,
    resetGame,
    rematch,
    drawScoreChart,
  }
}

