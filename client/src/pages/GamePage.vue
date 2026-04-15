<script setup lang="ts">
import { ref, computed, onMounted, onBeforeUnmount } from 'vue'
import * as signalR from '@microsoft/signalr'

const props = defineProps<{ id: string }>()

/* ───── Interfaces ───── */
interface Team { id: string; name: string }
interface GamePlayer {
  playerId: string; playerName: string
  teamId: string | null; teamName: string | null
  joinedAt: string; claimedByConnectionId: string | null
}
interface ScoreEntry {
  id: string; playerId: string; playerName: string
  teamId: string | null; round: number | null
  value: number; cumulativeValue: number | null; createdAt: string
}
interface Game {
  id: string; name: string; status: string
  lowerIsBetter: boolean; maxRounds: number | null
  startingScore: number; winnerId: string | null
  createdAt: string; finishedAt: string | null
  creatorOnly: boolean; scoreIncrement: number
  teamBasedWinner: boolean; currentRound: number
  gameMode: string | null; gameModeValue: number | null
  teams: Team[]; players: GamePlayer[]; scores: ScoreEntry[]
}

/* ───── State ───── */
const game = ref<Game | null>(null)
const loading = ref(true)

// Form inputs
const newTeamName = ref('')
const newPlayerName = ref('')
const selectedTeamId = ref<string | null>(null)

// SignalR
const connection = ref<signalR.HubConnection | null>(null)
const claim = ref<{ gameId: string; playerId: string | null; playerName: string | null; role: string } | null>(null)
const showClaimPicker = ref(false)

// Lobby-inställningar (editerbara)
const lobbyMaxRounds = ref<number | null>(1)
const lobbyIncrement = ref<number>(1)
const lobbyLowerIsBetter = ref(false)
const lobbyCreatorOnly = ref(false)
const lobbyTeamBasedWinner = ref(false)
const lobbyGameMode = ref<string | null>(null)
const lobbyGameModeValue = ref<number | null>(null)

/* ───── Computed ───── */
const isCreator = computed(() => {
  const secret = localStorage.getItem(`creator:${props.id}`)
  return !!secret
})

const canEdit = computed(() => {
  if (!game.value) return false
  if (!game.value.creatorOnly) return true
  return isCreator.value
})

/** Kan denna klient redigera en specifik spelares poäng? */
function canEditPlayer(playerId: string): boolean {
  if (!game.value) return false
  // Alla kan alltid redigera sin egen claimade spelares poäng
  if (claim.value?.playerId === playerId) return true
  // Om creatorOnly → bara creator kan redigera andras
  if (game.value.creatorOnly) return isCreator.value
  // Annars kan alla redigera alla
  return true
}

/** Man kan bara byta spelare/unclaima om spelet är pausat (Waiting) */
const canSwitchClaim = computed(() => {
  if (!game.value) return false
  return game.value.status === 'Waiting'
})

const shareUrl = computed(() => `${window.location.origin}/games/${props.id}`)

const rounds = computed(() => {
  if (!game.value) return []
  const max = game.value.maxRounds ?? 1
  return Array.from({ length: max }, (_, i) => i + 1)
})

const scoreMatrix = computed(() => {
  if (!game.value) return {} as Record<string, Record<number, ScoreEntry>>
  const matrix: Record<string, Record<number, ScoreEntry>> = {}
  for (const p of game.value.players) matrix[p.playerId] = {}
  for (const s of game.value.scores) {
    if (s.round != null && matrix[s.playerId])
      matrix[s.playerId][s.round] = s
  }
  return matrix
})

function getScoreValue(playerId: string, round: number): number {
  return scoreMatrix.value[playerId]?.[round]?.value ?? 0
}

function playerRawTotal(playerId: string): number {
  return game.value?.scores
    .filter(s => s.playerId === playerId)
    .reduce((sum, s) => sum + s.value, 0) ?? 0
}

/** Visar aktuellt ställning med StartingScore (t.ex. 501 - total för dart) */
function playerDisplayTotal(playerId: string): number {
  const raw = playerRawTotal(playerId)
  const start = game.value?.startingScore ?? 0
  return start !== 0 ? start - raw : raw
}

const scoreboard = computed(() => {
  if (!game.value) return []
  const map = new Map<string, { playerId: string; name: string; teamName: string | null; total: number; displayTotal: number }>()
  for (const p of game.value.players) {
    map.set(p.playerId, {
      playerId: p.playerId, name: p.playerName, teamName: p.teamName,
      total: playerRawTotal(p.playerId),
      displayTotal: playerDisplayTotal(p.playerId)
    })
  }
  const arr = Array.from(map.values())
  arr.sort((a, b) => game.value!.lowerIsBetter ? a.displayTotal - b.displayTotal : b.displayTotal - a.displayTotal)
  return arr
})

const winnerName = computed(() => {
  if (!game.value?.winnerId) return null
  if (game.value.teamBasedWinner) {
    const t = game.value.teams.find(t => t.id === game.value!.winnerId)
    return t?.name ?? 'Okänt lag'
  }
  const p = game.value.players.find(pl => pl.playerId === game.value!.winnerId)
  return p?.playerName ?? 'Okänd'
})

function statusBadge(status: string) {
  return `badge badge-${status.toLowerCase()}`
}

function isPlayerClaimed(p: GamePlayer): boolean {
  return p.claimedByConnectionId != null
}

function isClaimedByMe(p: GamePlayer): boolean {
  return p.claimedByConnectionId === connection.value?.connectionId
}

/* ───── API calls ───── */
async function fetchGame() {
  const res = await fetch(`/api/games/${props.id}`)
  if (!res.ok) return
  game.value = await res.json()
  if (game.value) {
    lobbyMaxRounds.value = game.value.maxRounds
    lobbyIncrement.value = game.value.scoreIncrement
    lobbyLowerIsBetter.value = game.value.lowerIsBetter
    lobbyCreatorOnly.value = game.value.creatorOnly
    lobbyTeamBasedWinner.value = game.value.teamBasedWinner
    lobbyGameMode.value = game.value.gameMode
    lobbyGameModeValue.value = game.value.gameModeValue
  }
}

async function load() {
  loading.value = true
  try { await fetchGame() }
  finally { loading.value = false }
}

async function saveSettings() {
  const secret = localStorage.getItem(`creator:${props.id}`)
  if (!secret) return
  await fetch(`/api/games/${props.id}/settings`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json', 'X-Creator-Secret': secret },
    body: JSON.stringify({
      maxRounds: lobbyMaxRounds.value,
      scoreIncrement: lobbyIncrement.value,
      lowerIsBetter: lobbyLowerIsBetter.value,
      creatorOnly: lobbyCreatorOnly.value,
      teamBasedWinner: lobbyTeamBasedWinner.value,
      gameMode: lobbyGameMode.value,
      gameModeValue: lobbyGameModeValue.value,
    }),
  })
  await fetchGame()
}

async function addTeam() {
  if (!newTeamName.value.trim()) return
  await fetch(`/api/games/${props.id}/teams`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ name: newTeamName.value }),
  })
  newTeamName.value = ''
  await fetchGame()
}

async function addPlayer() {
  if (!newPlayerName.value.trim()) return
  await fetch(`/api/games/${props.id}/players/new`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ userName: newPlayerName.value.trim(), teamId: selectedTeamId.value || null }),
  })
  newPlayerName.value = ''
  selectedTeamId.value = null
  await fetchGame()
}

async function assignTeam(playerId: string, teamId: string | null) {
  await fetch(`/api/games/${props.id}/players/team`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ playerId, teamId }),
  })
  await fetchGame()
}

async function adjustScore(playerId: string, round: number, delta: number) {
  const existing = scoreMatrix.value[playerId]?.[round]
  if (existing) {
    await fetch(`/api/games/${props.id}/scores`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ playerId, round, value: existing.value + delta }),
    })
  } else {
    const teamId = game.value?.players.find(p => p.playerId === playerId)?.teamId ?? null
    await fetch(`/api/games/${props.id}/scores`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ playerId, teamId, round, value: delta }),
    })
  }
  await fetchGame()
}

async function startGame() {
  await fetch(`/api/games/${props.id}/start`, { method: 'PUT' })
  await fetchGame()
}

async function pauseGame() {
  await fetch(`/api/games/${props.id}/pause`, { method: 'PUT' })
  await fetchGame()
}

async function finishGame() {
  await fetch(`/api/games/${props.id}/finish`, { method: 'PUT' })
  await fetchGame()
}

async function advanceRound() {
  await fetch(`/api/games/${props.id}/advance-round`, { method: 'PUT' })
  await fetchGame()
}

async function shareLink() {
  if (navigator.share) {
    await navigator.share({ title: `Match ${game.value?.name}`, text: 'Gå med i spelet!', url: shareUrl.value })
  } else {
    try {
      await navigator.clipboard.writeText(shareUrl.value)
      alert('Länk kopierad!')
    } catch { alert(shareUrl.value) }
  }
}

/* ───── SignalR ───── */
async function connectHub(playerId?: string) {
  const secret = localStorage.getItem(`creator:${props.id}`) ?? ''
  let url = `/gamehub?gameId=${props.id}`
  if (playerId) url += `&playerId=${playerId}`
  if (secret) url += `&creatorSecret=${secret}`

  connection.value = new signalR.HubConnectionBuilder()
    .withUrl(url)
    .withAutomaticReconnect()
    .build()

  connection.value.on('ClaimAccepted', (c) => {
    claim.value = c
    localStorage.setItem(`claim:${props.id}`, JSON.stringify(c))
  })

  connection.value.on('ClaimRejected', () => {
    showClaimPicker.value = true
  })

  connection.value.on('GameUpdated', async () => {
    await fetchGame()
  })

  connection.value.on('ScoreAdded', async () => {
    await fetchGame()
  })

  await connection.value.start()
}

async function claimPlayer(playerId: string | null) {
  showClaimPicker.value = false
  if (connection.value) {
    try { await connection.value.stop() } catch {}
  }
  await connectHub(playerId ?? undefined)
}

async function unclaimPlayer() {
  if (connection.value && claim.value?.playerId) {
    await connection.value.invoke('UnclaimPlayer', props.id, claim.value.playerId)
    claim.value = null
    localStorage.removeItem(`claim:${props.id}`)
    showClaimPicker.value = true
  }
}

/* ───── Lifecycle ───── */
onMounted(async () => {
  await load()
  const stored = localStorage.getItem(`claim:${props.id}`)
  if (stored) {
    const parsed = JSON.parse(stored)
    await connectHub(parsed.playerId ?? undefined)
  } else {
    showClaimPicker.value = true
    await connectHub()
  }
})

onBeforeUnmount(async () => {
  if (connection.value) {
    try {
      await connection.value.invoke('LeaveGame', props.id)
      await connection.value.stop()
    } catch {}
  }
})
</script>

<template>
  <div class="page">
    <router-link to="/" class="back-link">← Tillbaka</router-link>

    <div v-if="loading" class="card">Laddar...</div>
    <div v-else-if="!game" class="card"><h2>Spelet hittades inte</h2></div>

    <template v-else>
      <!-- ═══ HEADER (alltid synlig) ═══ -->
      <h1>{{ game.name }}</h1>
      <div class="game-meta">
        <span>Status: <span :class="statusBadge(game.status)">{{ game.status }}</span></span>
        <span v-if="game.startingScore">Start: <strong>{{ game.startingScore }}</strong></span>
        <span>Lägre = bättre: <strong>{{ game.lowerIsBetter ? 'Ja' : 'Nej' }}</strong></span>
        <span>Rundor: <strong>{{ game.maxRounds ?? '∞' }}</strong></span>
        <span>Inkrement: <strong>{{ game.scoreIncrement }}</strong></span>
        <span v-if="game.gameMode">
          Läge: <strong>{{ game.gameMode === 'BestOf' ? `Bäst av ${game.gameModeValue}` : `Först till ${game.gameModeValue}` }}</strong>
        </span>
      </div>

      <!-- Vinnarbanner (Finished) -->
      <div v-if="game.status === 'Finished'" class="winner-banner">
        <strong>🏆 Spelet avslutat!</strong>
        <span v-if="winnerName"> Vinnare: <strong>{{ winnerName }}</strong></span>
        <span v-if="game.teamBasedWinner"> (Lagvinst)</span>
        <br/><small class="text-muted">{{ new Date(game.finishedAt!).toLocaleString('sv-SE') }}</small>
      </div>

      <!-- ═══ CLAIM PICKER ═══ -->
      <div v-if="showClaimPicker && game.players.length && (canSwitchClaim || !claim)" class="card">
        <h2>Vem är du?</h2>
        <div class="claim-buttons">
          <button
            v-for="p in game.players" :key="p.playerId"
            :class="isPlayerClaimed(p) ? 'btn-claimed' : 'btn-primary'"
            :disabled="isPlayerClaimed(p) && !isClaimedByMe(p)"
            @click="claimPlayer(p.playerId)"
          >
            {{ p.playerName }}
            <span v-if="isPlayerClaimed(p)" class="claimed-tag">✓ tagen</span>
          </button>
          <button v-if="canSwitchClaim" class="btn-secondary" @click="claimPlayer(null)">Åskådare</button>
        </div>
      </div>

      <!-- Aktuell claim-info -->
      <div v-if="claim && !showClaimPicker" class="claim-info">
        <span v-if="claim.role === 'creator'">Du är speladmin</span>
        <span v-else-if="claim.role === 'player'">Inloggad som <strong>{{ claim.playerName }}</strong></span>
        <span v-else>👁 Åskådare</span>
        <button v-if="canSwitchClaim" class="btn-sm btn-secondary" @click="unclaimPlayer">Byt</button>
      </div>

      <!-- ==================================== -->
      <!-- ===        WAITING / LOBBY       === -->
      <!-- ==================================== -->
      <template v-if="game.status === 'Waiting'">

        <!-- Lobby-inställningar (bara creator) -->
        <div v-if="isCreator" class="card">
          <h2>⚙ Spelinställningar</h2>
          <div class="lobby-settings">
            <div class="form-row">
              <div class="label">
                Max rundor
                <input type="number" v-model.number="lobbyMaxRounds" min="1" @change="saveSettings" />
              </div>
              <div class="label">
                Poäng per klick
                <input type="number" v-model.number="lobbyIncrement" min="0.1" step="any" @change="saveSettings" />
              </div>
            </div>
            <div class="form-row" style="margin-top: 8px;">
              <label><input type="checkbox" v-model="lobbyLowerIsBetter" @change="saveSettings" /> Lägre = bättre</label>
              <label><input type="checkbox" v-model="lobbyCreatorOnly" @change="saveSettings" /> Bara jag redigerar poäng</label>
              <label v-if="game.teams.length"><input type="checkbox" v-model="lobbyTeamBasedWinner" @change="saveSettings" /> Lagvinnare</label>
            </div>
            <div class="form-row" style="margin-top: 8px;">
              <div class="label">
                Spelläge
                <select v-model="lobbyGameMode" @change="saveSettings">
                  <option :value="null">Standard</option>
                  <option value="BestOf">Bäst av X</option>
                  <option value="FirstTo">Först till X</option>
                </select>
              </div>
              <div v-if="lobbyGameMode" class="label">
                {{ lobbyGameMode === 'BestOf' ? 'Antal rundor' : 'Poängmål' }}
                <input type="number" v-model.number="lobbyGameModeValue" min="1" @change="saveSettings" />
              </div>
            </div>
          </div>
        </div>

        <!-- Lag -->
        <div class="card">
          <h2>Lag</h2>
          <ul v-if="game.teams.length" class="player-list">
            <li v-for="t in game.teams" :key="t.id">{{ t.name }}</li>
          </ul>
          <p v-else class="empty">Inga lag (individuellt spel)</p>
          <form v-if="isCreator" @submit.prevent="addTeam" class="form-row" style="margin-top: 10px;">
            <input v-model="newTeamName" placeholder="Lagnamn" />
            <button type="submit" class="btn-primary">+ Lag</button>
          </form>
        </div>

        <!-- Spelare -->
        <div class="card">
          <h2>Spelare</h2>
          <table v-if="game.players.length">
            <thead>
              <tr>
                <th>Namn</th>
                <th>Lag</th>
                <th>Status</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="p in game.players" :key="p.playerId">
                <td>{{ p.playerName }}</td>
                <td>
                  <select v-if="isCreator && game.teams.length" :value="p.teamId" @change="assignTeam(p.playerId, ($event.target as HTMLSelectElement).value || null)">
                    <option :value="''">Inget lag</option>
                    <option v-for="t in game.teams" :key="t.id" :value="t.id">{{ t.name }}</option>
                  </select>
                  <span v-else>{{ p.teamName ?? '–' }}</span>
                </td>
                <td>
                  <span v-if="isPlayerClaimed(p)" class="badge badge-active">Ansluten</span>
                  <span v-else class="badge badge-waiting">Väntar</span>
                </td>
              </tr>
            </tbody>
          </table>
          <p v-else class="empty">Inga spelare ännu.</p>

          <form @submit.prevent="addPlayer" class="form-row" style="margin-top: 10px;">
            <input v-model="newPlayerName" placeholder="Spelarnamn" />
            <select v-model="selectedTeamId" v-if="game.teams.length">
              <option :value="null">Inget lag</option>
              <option v-for="t in game.teams" :key="t.id" :value="t.id">{{ t.name }}</option>
            </select>
            <button type="submit" class="btn-primary" :disabled="!newPlayerName.trim()">+ Spelare</button>
          </form>
        </div>

        <!-- Start / Dela -->
        <div class="card action-bar">
          <button v-if="isCreator" class="btn-success btn-lg" @click="startGame" :disabled="game.players.length < 2">
            ▶ Starta spel
          </button>
          <span v-if="game.players.length < 2" class="text-muted">Minst 2 spelare krävs</span>
          <button @click="shareLink" class="btn-copy">📋 Kopiera länk</button>
        </div>
      </template>

      <!-- ==================================== -->
      <!-- ===         ACTIVE               === -->
      <!-- ==================================== -->
      <template v-if="game.status === 'Active'">

        <!-- Kontrollpanel -->
        <div class="card action-bar">
          <div class="round-indicator">
            Runda <strong>{{ game.currentRound }}</strong> av {{ game.maxRounds ?? '∞' }}
          </div>
          <button v-if="canEdit" class="btn-primary" @click="advanceRound"
                  :disabled="game.maxRounds != null && game.currentRound >= game.maxRounds">
            Nästa runda →
          </button>
          <button v-if="isCreator" class="btn-warning" @click="pauseGame">⏸ Pausa</button>
          <button v-if="isCreator" class="btn-danger" @click="finishGame">⏹ Avsluta</button>
          <button @click="shareLink" class="btn-copy">📋 Länk</button>
        </div>

        <!-- Ställning -->
        <div class="card">
          <h2>Ställning</h2>
          <table v-if="scoreboard.length">
            <thead>
              <tr>
                <th>#</th>
                <th>Spelare</th>
                <th v-if="game.teams.length">Lag</th>
                <th class="text-right">{{ game.startingScore ? 'Kvar' : 'Total' }}</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="(s, i) in scoreboard" :key="s.playerId" :class="{ 'winner-row': i === 0 && game.scores.length > 0 }">
                <td>{{ i + 1 }}</td>
                <td>{{ s.name }} <span v-if="i === 0 && game.scores.length > 0">👑</span></td>
                <td v-if="game.teams.length">{{ s.teamName ?? '–' }}</td>
                <td class="text-right">{{ s.displayTotal }}</td>
              </tr>
            </tbody>
          </table>
          <p v-else class="empty">Inga poäng ännu.</p>
        </div>

        <!-- Poängmatris -->
        <div v-if="game.players.length" class="card">
          <h2>Poängmatris</h2>
          <div class="score-matrix-wrapper">
            <table class="score-matrix">
              <thead>
                <tr>
                  <th class="round-col">Runda</th>
                  <th v-for="p in game.players" :key="p.playerId" class="player-col">{{ p.playerName }}</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="r in rounds" :key="r" :class="{ 'active-round': r === game.currentRound }">
                  <td class="round-label">
                    R{{ r }}
                    <span v-if="r === game.currentRound" class="round-badge">←</span>
                  </td>
                  <td v-for="p in game.players" :key="p.playerId" class="score-cell-td">
                    <div v-if="canEditPlayer(p.playerId)" class="score-cell">
                      <button class="sc-btn sc-minus" @click="adjustScore(p.playerId, r, -game.scoreIncrement)">−</button>
                      <span class="sc-val">{{ getScoreValue(p.playerId, r) }}</span>
                      <button class="sc-btn sc-plus" @click="adjustScore(p.playerId, r, game.scoreIncrement)">+</button>
                    </div>
                    <span v-else class="sc-val-readonly">{{ getScoreValue(p.playerId, r) }}</span>
                  </td>
                </tr>
              </tbody>
              <tfoot>
                <tr>
                  <td class="round-label">{{ game.startingScore ? 'Kvar' : 'Total' }}</td>
                  <td v-for="p in game.players" :key="p.playerId" class="total-val">
                    {{ playerDisplayTotal(p.playerId) }}
                  </td>
                </tr>
              </tfoot>
            </table>
          </div>
        </div>
      </template>

      <!-- ==================================== -->
      <!-- ===        FINISHED              === -->
      <!-- ==================================== -->
      <template v-if="game.status === 'Finished'">

        <!-- Slutställning -->
        <div class="card">
          <h2>Slutställning</h2>
          <table v-if="scoreboard.length">
            <thead>
              <tr>
                <th>#</th>
                <th>Spelare</th>
                <th v-if="game.teams.length">Lag</th>
                <th class="text-right">{{ game.startingScore ? 'Kvar' : 'Total' }}</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="(s, i) in scoreboard" :key="s.playerId" :class="{ 'winner-row': s.playerId === game.winnerId }">
                <td>{{ i + 1 }}</td>
                <td>
                  {{ s.name }}
                  <span v-if="s.playerId === game.winnerId"> 🏆</span>
                </td>
                <td v-if="game.teams.length">{{ s.teamName ?? '–' }}</td>
                <td class="text-right">{{ s.displayTotal }}</td>
              </tr>
            </tbody>
          </table>
        </div>

        <!-- Poängmatris (read-only) -->
        <div v-if="game.players.length" class="card">
          <h2>Poänghistorik</h2>
          <div class="score-matrix-wrapper">
            <table class="score-matrix">
              <thead>
                <tr>
                  <th class="round-col">Runda</th>
                  <th v-for="p in game.players" :key="p.playerId" class="player-col">{{ p.playerName }}</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="r in rounds" :key="r">
                  <td class="round-label">R{{ r }}</td>
                  <td v-for="p in game.players" :key="p.playerId" class="score-cell-td">
                    <span class="sc-val-readonly">{{ getScoreValue(p.playerId, r) }}</span>
                  </td>
                </tr>
              </tbody>
              <tfoot>
                <tr>
                  <td class="round-label">{{ game.startingScore ? 'Kvar' : 'Total' }}</td>
                  <td v-for="p in game.players" :key="p.playerId" class="total-val">
                    {{ playerDisplayTotal(p.playerId) }}
                  </td>
                </tr>
              </tfoot>
            </table>
          </div>
        </div>
      </template>

    </template>
  </div>
</template>

