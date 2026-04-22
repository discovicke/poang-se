import {computed, type Ref} from 'vue'
import type {Game, ScoreEntry, ScoreboardRow} from '../types/game'

/**
 * Composable härleder all beräknad speldata (scoreboard, score matrix,
 * round list, totals, winner name, etc.) från en reaktiv Game ref.
 */
export function useGameState(game: Ref<Game | null>, gameId: string) {

  /** Är det aktuella webbläsaren spelarens skapare? */
  const isCreator = computed(() => {
    return !!localStorage.getItem(`creator:${gameId}`)
  })

  /** Kan aktuell användare redigera poäng? */
  const canEdit = computed(() => {
    if (!game.value) return false
    if (!game.value.creatorOnly) return true
    return isCreator.value
  })

  /** Kan spelare byta claim? Endast när spelet är i väntande status. */
  const canSwitchClaim = computed(() => {
    return game.value?.status === 'Waiting'
  })

  /** Delbart URL för detta spelet. */
  const shareUrl = computed(() => `${window.location.origin}/games/${gameId}`)

  /** Array av rundnummer. Rundbaserade lägen: [1..currentRound] (dynamiskt). Annars: [1..maxRounds]. */
  const rounds = computed(() => {
    if (!game.value) return []
    const isRoundBased = game.value.gameMode === 'BestOf' ||
      (game.value.gameMode === 'FirstTo' && game.value.gameModeTarget === 'rounds')
    const max = isRoundBased
      ? game.value.currentRound
      : (game.value.maxRounds ?? 1)
    return Array.from({length: max}, (_, i) => i + 1)
  })

  /** Matrix: { [playerId]: { [round]: ScoreEntry } } */
  const scoreMatrix = computed(() => {
    if (!game.value) return {} as Record<string, Record<number, ScoreEntry>>
    const matrix: Record<string, Record<number, ScoreEntry>> = {}
    for (const p of game.value.players) {
      matrix[p.playerId] = {}
    }
    for (const s of game.value.scores) {
      if (s.round != null && matrix[s.playerId])
        matrix[s.playerId][s.round] = s
    }
    return matrix
  })

  /** Hämtar ett enskilt poängvärde */
  function getScoreValue(playerId: string, round: number): number {
    return scoreMatrix.value[playerId]?.[round]?.value ?? 0
  }

  /** Summan av råpoängvärden för en spelare. */
  function playerRawTotal(playerId: string): number {
    return game.value?.scores
      .filter(s => s.playerId === playerId)
      .reduce((sum, s) => sum + s.value, 0) ?? 0
  }

  /** Display total: räknar med startingScore (t.ex. 501 för dart). */
  function playerDisplayTotal(playerId: string): number {
    const raw = playerRawTotal(playerId)
    const start = game.value?.startingScore ?? 0
    return start !== 0
      ? start - raw
      : raw
  }

  /** Sorterade rader i scoreboard. Team-aggregerat när teamBasedWinner är satt. */
  const scoreboard = computed<ScoreboardRow[]>(() => {
    if (!game.value) return []

    if (game.value.teamBasedWinner && game.value.teams.length > 0) {
      // Aggregera poäng per lag
      const rows: ScoreboardRow[] = game.value.teams.map(team => {
        const teamPlayers = game.value!.players.filter(p => p.teamId === team.id)
        const raw = teamPlayers.reduce((sum, p) => sum + playerRawTotal(p.playerId), 0)
        const start = game.value!.startingScore ?? 0
        const displayTotal = start !== 0 ? start - raw : raw
        return {
          rowId: team.id,
          playerId: team.id,
          name: team.name,
          teamName: null,
          memberNames: teamPlayers.map(p => p.playerName),
          total: raw,
          displayTotal,
        }
      })
      rows.sort((a, b) =>
        game.value!.lowerIsBetter
          ? a.displayTotal - b.displayTotal
          : b.displayTotal - a.displayTotal,
      )
      return rows
    }

    // Per-spelare (standard)
    const arr: ScoreboardRow[] = game.value.players.map(p => ({
      rowId: p.playerId,
      playerId: p.playerId,
      name: p.playerName,
      teamName: p.teamName,
      total: playerRawTotal(p.playerId),
      displayTotal: playerDisplayTotal(p.playerId),
    }))
    arr.sort((a, b) =>
      game.value!.lowerIsBetter
        ? a.displayTotal - b.displayTotal
        : b.displayTotal - a.displayTotal,
    )
    return arr
  })

  /** Hämtar vinnarens namn (spelare eller lag). */
  const winnerName = computed(() => {
    if (!game.value?.winnerId)
      return null

    if (game.value.teamBasedWinner) {
      const t = game.value.teams.find(t => t.id === game.value!.winnerId)
      return t?.name ?? 'Okänt lag'
    }

    const p = game.value.players.find(pl => pl.playerId === game.value!.winnerId)
    return p?.playerName ?? 'Okänd'
  })

  return {
    isCreator,
    canEdit,
    canSwitchClaim,
    shareUrl,
    rounds,
    scoreMatrix,
    getScoreValue,
    playerRawTotal,
    playerDisplayTotal,
    scoreboard,
    winnerName,
  }
}

