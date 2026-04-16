/* ---- Delade interfaces för speldomänen ---- */

export interface Team {
  id: string
  name: string
}

export interface GamePlayer {
  playerId: string
  playerName: string
  teamId: string | null
  teamName: string | null
  joinedAt: string
  claimedByConnectionId: string | null
}

export interface ScoreEntry {
  id: string
  playerId: string
  playerName: string
  teamId: string | null
  round: number | null
  value: number
  cumulativeValue: number | null
  createdAt: string
}

export interface Game {
  id: string
  name: string
  status: string
  isPrivate: boolean
  lowerIsBetter: boolean
  maxRounds: number | null
  startingScore: number
  winnerId: string | null
  createdAt: string
  finishedAt: string | null
  creatorOnly: boolean
  scoreIncrement: number
  teamBasedWinner: boolean
  currentRound: number
  gameMode: string | null
  gameModeValue: number | null
  teams: Team[]
  players: GamePlayer[]
  scores: ScoreEntry[]
}

export interface ClaimInfo {
  gameId: string
  playerId: string | null
  playerName: string | null
  role: string
}

export interface ScoreboardRow {
  playerId: string
  name: string
  teamName: string | null
  total: number
  displayTotal: number
}

