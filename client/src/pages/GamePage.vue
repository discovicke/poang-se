<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'

const props = defineProps<{ id: string }>()

interface Team {
  id: string
  name: string
}

interface GamePlayer {
  playerId: string
  playerName: string
  teamId: string | null
  teamName: string | null
  joinedAt: string
}

interface ScoreEntry {
  id: string
  playerId: string
  playerName: string
  teamId: string | null
  round: number | null
  value: number
  cumulativeValue: number | null
  createdAt: string
}

interface Game {
  id: string
  name: string
  status: string
  lowerIsBetter: boolean
  maxRounds: number | null
  startingScore: number
  winnerId: string | null
  createdAt: string
  finishedAt: string | null
  teams: Team[]
  players: GamePlayer[]
  scores: ScoreEntry[]
}

const game = ref<Game | null>(null)
const loading = ref(true)

// Form inputs
const newTeamName = ref('')
const newPlayerName = ref('')
const selectedTeamId = ref<string | null>(null)
const scorePlayerId = ref('')
const scoreValue = ref<number>(0)
const scoreRound = ref<number | null>(null)
const scoreTeamId = ref<string | null>(null)

async function fetchGame() {
  const res = await fetch(`/api/games/${props.id}`)
  if (!res.ok) return
  game.value = await res.json()
}

async function load() {
  loading.value = true
  await fetchGame()
  loading.value = false
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
    body: JSON.stringify({
      userName: newPlayerName.value.trim(),
      teamId: selectedTeamId.value || null,
    }),
  })
  newPlayerName.value = ''
  selectedTeamId.value = null
  await fetchGame()
}

async function addScore() {
  if (!scorePlayerId.value) return
  await fetch(`/api/games/${props.id}/scores`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      playerId: scorePlayerId.value,
      teamId: scoreTeamId.value || null,
      round: scoreRound.value,
      value: scoreValue.value,
    }),
  })
  scoreValue.value = 0
  scoreRound.value = null
  await fetchGame()
}

async function startGame() {
  await fetch(`/api/games/${props.id}/start`, { method: 'PUT' })
  await fetchGame()
}

async function finishGame() {
  await fetch(`/api/games/${props.id}/finish`, { method: 'PUT' })
  await fetchGame()
}

const scoreboard = computed(() => {
  if (!game.value) return []
  const map = new Map<string, { playerId: string; name: string; teamName: string | null; total: number }>()
  for (const p of game.value.players) {
    map.set(p.playerId, { playerId: p.playerId, name: p.playerName, teamName: p.teamName, total: 0 })
  }
  for (const s of game.value.scores) {
    const entry = map.get(s.playerId)
    if (entry) entry.total = s.cumulativeValue ?? entry.total
  }
  const arr = Array.from(map.values())
  arr.sort((a, b) => game.value!.lowerIsBetter ? a.total - b.total : b.total - a.total)
  return arr
})

const winnerName = computed(() => {
  if (!game.value?.winnerId) return null
  const p = game.value.players.find((pl) => pl.playerId === game.value!.winnerId)
  return p?.playerName ?? 'Okänd'
})

function statusBadge(status: string) {
  return `badge badge-${status.toLowerCase()}`
}

onMounted(load)
</script>

<template>
  <div class="page">
    <router-link to="/" class="back-link">← Tillbaka</router-link>

    <div v-if="loading" class="card">Laddar...</div>

    <div v-else-if="!game" class="card">
      <h2>Spelet hittades inte</h2>
    </div>

    <template v-else>
      <!-- Header -->
      <h1>{{ game.name }}</h1>
      <div class="game-meta">
        <span>Status: <span :class="statusBadge(game.status)">{{ game.status }}</span></span>
        <span>Lägre = bättre: <strong>{{ game.lowerIsBetter ? 'Ja' : 'Nej' }}</strong></span>
        <span>Max rundor: <strong>{{ game.maxRounds ?? '∞' }}</strong></span>
        <span v-if="game.startingScore !== 0">Startpoäng: <strong>{{ game.startingScore }}</strong></span>
      </div>

      <!-- Banner för vinnare i slutet av spelet -->
      <div v-if="game.status === 'Finished'" class="winner-banner">
        <strong>Spelet avslutat!</strong>
        <span v-if="winnerName"> Vinnare: <strong>{{ winnerName }}</strong></span>
        <br />
        <small class="text-muted">Avslutat: {{ new Date(game.finishedAt!).toLocaleString('sv-SE') }}</small>
      </div>

      <!-- Status kontroller -->
      <div v-if="game.status === 'Waiting'" class="card">
        <button class="btn-success" @click="startGame">Starta spel</button>
      </div>
      <div v-if="game.status === 'Active'" class="card">
        <button class="btn-danger" @click="finishGame">Avsluta spel</button>
      </div>

      <!-- Lag -->
      <div class="card">
        <h2>Lag</h2>
        <ul v-if="game.teams.length" class="player-list">
          <li v-for="t in game.teams" :key="t.id">{{ t.name }}</li>
        </ul>
        <p v-else class="empty">Inga lag (individuellt spel)</p>
        <form v-if="game.status === 'Waiting'" @submit.prevent="addTeam" class="form-row">
          <input v-model="newTeamName" placeholder="Lagnamn" />
          <button type="submit" class="btn-primary">Lägg till lag</button>
        </form>
      </div>

      <!-- Spelare -->
      <div class="card">
        <h2>Spelare i spelet</h2>
        <table v-if="game.players.length">
          <thead>
            <tr>
              <th>Spelare</th>
              <th>Lag</th>
              <th>Gick med</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="p in game.players" :key="p.playerId">
              <td>{{ p.playerName }}</td>
              <td>{{ p.teamName ?? '–' }}</td>
              <td class="text-muted">{{ new Date(p.joinedAt).toLocaleString('sv-SE') }}</td>
            </tr>
          </tbody>
        </table>
        <p v-else class="empty">Inga spelare ännu.</p>

        <form v-if="game.status !== 'Finished'" @submit.prevent="addPlayer" class="form-row">
          <input v-model="newPlayerName" placeholder="Spelarnamn" />
          <select v-model="selectedTeamId" v-if="game.teams.length">
            <option :value="null">Inget lag</option>
            <option v-for="t in game.teams" :key="t.id" :value="t.id">{{ t.name }}</option>
          </select>
          <button type="submit" class="btn-primary" :disabled="!newPlayerName.trim()">Lägg till spelare</button>
        </form>
      </div>

      <!-- Poängställning -->
      <div class="card">
        <h2>Ställning</h2>
        <table v-if="scoreboard.length">
          <thead>
            <tr>
              <th>#</th>
              <th>Spelare</th>
              <th>Lag</th>
              <th class="text-right">Total</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(s, i) in scoreboard" :key="s.playerId" :class="{ 'winner-row': s.playerId === game.winnerId }">
              <td>{{ i + 1 }}</td>
              <td>
                {{ s.name }}
                <span v-if="s.playerId === game.winnerId"> 🏆</span>
              </td>
              <td>{{ s.teamName ?? '–' }}</td>
              <td class="text-right">{{ s.total }}</td>
            </tr>
          </tbody>
        </table>
        <p v-else class="empty">Inga poäng ännu.</p>
      </div>

      <!-- Lägga till poäng -->
      <div v-if="game.status === 'Active'" class="card">
        <h2>Lägg till poäng</h2>
        <form @submit.prevent="addScore" class="form-row">
          <div class="label">
            Spelare
            <select v-model="scorePlayerId">
              <option value="" disabled>Välj spelare...</option>
              <option v-for="p in game.players" :key="p.playerId" :value="p.playerId">{{ p.playerName }}</option>
            </select>
          </div>
          <div v-if="game.teams.length" class="label">
            Lag
            <select v-model="scoreTeamId">
              <option :value="null">–</option>
              <option v-for="t in game.teams" :key="t.id" :value="t.id">{{ t.name }}</option>
            </select>
          </div>
          <div class="label">
            Runda
            <input type="number" v-model.number="scoreRound" min="1" />
          </div>
          <div class="label">
            Poäng
            <input type="number" v-model.number="scoreValue" step="any" />
          </div>
          <button type="submit" class="btn-success" :disabled="!scorePlayerId">Registrera poäng</button>
        </form>
      </div>

      <!-- Poänghistorik -->
      <div class="card">
        <h2>Poänghistorik</h2>
        <table v-if="game.scores.length">
          <thead>
            <tr>
              <th>Runda</th>
              <th>Spelare</th>
              <th class="text-right">Poäng</th>
              <th class="text-right">Ack. total</th>
              <th>Tid</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="s in game.scores" :key="s.id">
              <td>{{ s.round ?? '–' }}</td>
              <td>{{ s.playerName }}</td>
              <td class="text-right">{{ s.value }}</td>
              <td class="text-right">{{ s.cumulativeValue }}</td>
              <td class="text-muted">{{ new Date(s.createdAt).toLocaleString('sv-SE') }}</td>
            </tr>
          </tbody>
        </table>
        <p v-else class="empty">Inga poäng registrerade ännu.</p>
      </div>
    </template>
  </div>
</template>

