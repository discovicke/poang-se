import {ref} from 'vue'
import type {Game} from '../types/game'

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
    return token ? {Authorization: `Bearer ${token}`} : {}
  }

  async function fetchGame(): Promise<Game | null> {
    const res = await fetch(base, {headers: authHeaders()})
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
      headers: {'Content-Type': 'application/json'},
      body: JSON.stringify({password}),
    })
    if (!res.ok)
      return false
    const {token} = await res.json()
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
    },
  ): Promise<Game | null> {
    await fetch(`${base}/settings`, {
      method: 'PUT',
      headers: {'Content-Type': 'application/json', 'X-Creator-Secret': secret},
      body: JSON.stringify(settings),
    })
    return fetchGame()
  }

  async function addTeam(name: string): Promise<Game | null> {
    await fetch(`${base}/teams`, {
      method: 'POST',
      headers: {'Content-Type': 'application/json'},
      body: JSON.stringify({name}),
    })
    return fetchGame()
  }

  async function addPlayer(userName: string, teamId: string | null): Promise<Game | null> {
    await fetch(`${base}/players/new`, {
      method: 'POST',
      headers: {'Content-Type': 'application/json'},
      body: JSON.stringify({userName, teamId: teamId || null}),
    })
    return fetchGame()
  }

  async function assignTeam(playerId: string, teamId: string | null): Promise<Game | null> {
    await fetch(`${base}/players/team`, {
      method: 'PUT',
      headers: {'Content-Type': 'application/json'},
      body: JSON.stringify({playerId, teamId}),
    })
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
      headers: {'Content-Type': 'application/json'},
      body: JSON.stringify({playerId, teamId, round, value}),
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
      headers: {'Content-Type': 'application/json'},
      body: JSON.stringify({playerId, round, value}),
    })
    return fetchGame()
  }

  async function startGame(): Promise<Game | null> {
    await fetch(`${base}/start`, {method: 'PUT'})
    return fetchGame()
  }

  async function pauseGame(): Promise<Game | null> {
    await fetch(`${base}/pause`, {method: 'PUT'})
    return fetchGame()
  }

  async function finishGame(): Promise<Game | null> {
    await fetch(`${base}/finish`, {method: 'PUT'})
    return fetchGame()
  }

  async function advanceRound(): Promise<Game | null> {
    await fetch(`${base}/advance-round`, {method: 'PUT'})
    return fetchGame()
  }

  return {
    loading,
    fetchGame,
    saveSettings,
    addTeam,
    addPlayer,
    assignTeam,
    addScore,
    updateScore,
    startGame,
    pauseGame,
    finishGame,
    advanceRound,
  }
}

