<script setup lang="ts">
import {ref, onMounted, onBeforeUnmount} from 'vue'
import type {Game} from '../types/game'

import {useGameApi} from '../composables/useGameApi'
import {useSignalR} from '../composables/useSignalR'
import {useGameState} from '../composables/useGameState'

import PageNotFound from './PageNotFound.vue'
import GameHeader from '../components/GameHeader.vue'
import ClaimPicker from '../components/ClaimPicker.vue'
import GameLobby from '../components/GameLobby.vue'
import GameControls from '../components/GameControls.vue'
import Scoreboard from '../components/Scoreboard.vue'
import ScoreMatrix from '../components/ScoreMatrix.vue'

const props = defineProps<{ id: string }>()

/* -- Composables -- */
const api = useGameApi(props.id)
const hub = useSignalR(props.id)

const game = ref<Game | null>(null)
const state = useGameState(game, props.id)

/* -- Refresh helper (Används av SignalR för callbacks också) -- */
async function refresh() {
  const g = await api.fetchGame()
  if (g)
    game.value = g
}

/* -- Lobby funktioner -- */
async function onSaveSettings(settings: Record<string, unknown>) {
  const secret = localStorage.getItem(`creator:${props.id}`)

  if (!secret)
    return

  const g = await api.saveSettings(secret, settings as any)
  if (g)
    game.value = g
}

async function onAddTeam(name: string) {
  const g = await api.addTeam(name)
  if (g)
    game.value = g
const loading = ref(true)

// Form inputs
const newTeamName = ref('')
const newPlayerName = ref('')
const selectedTeamId = ref<string | null>(null)
const scorePlayerId = ref('')
const scoreValue = ref<number>(0)
const scoreRound = ref<number | null>(null)
const scoreTeamId = ref<string | null>(null)

const connection = ref<signalR.HubConnection | null>(null)
const claim = ref<{ gameId: string; playerId: string | null; playerName: string | null; role: string } | null>(null)
const showClaimPicker = ref(false)

const isLocked = ref(false)
const lockedGameName = ref('')
const lockPassword = ref('')

function authHeaders(): Record<string, string> {
  const headers: Record<string, string> = { 'Content-Type': 'application/json' }
  const token = localStorage.getItem(`gameToken:${props.id}`)
  if (token) headers['Authorization'] = `Bearer ${token}`
  return headers
}

async function fetchGame() {
  const res = await fetch(`/api/games/${props.id}`, { headers: authHeaders() })
  if (res.status === 403) {
    const data = await res.json()
    if (data.isPrivate) {
      isLocked.value = true
      lockedGameName.value = data.name ?? ''
    }
    return
  }
  if (!res.ok) return
  game.value = await res.json()
  isLocked.value = false
}

async function unlock() {
  const res = await fetch(`/api/games/${props.id}/unlock`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ password: lockPassword.value }),
  })
  if (res.ok) {
    const { token } = await res.json()
    localStorage.setItem(`gameToken:${props.id}`, token)
    lockPassword.value = ''
    await fetchGame()
  } else {
    alert('Fel lösenord. Försök igen.')
  }
}

async function load() {
  loading.value = true
  try {
    await fetchGame()
  } finally {
    loading.value = false
  }
}
async function addTeam() {
  if (!newTeamName.value.trim()) return
  await fetch(`/api/games/${props.id}/teams`, {
    method: 'POST',
    headers: authHeaders(),
    body: JSON.stringify({name: newTeamName.value}),
  })
  newTeamName.value = ''
  await fetchGame()
}

async function onAddPlayer(name: string, teamId: string | null) {
  const g = await api.addPlayer(name, teamId)
  if (g)
    game.value = g
async function addPlayer() {
  if (!newPlayerName.value.trim()) return
  await fetch(`/api/games/${props.id}/players/new`, {
    method: 'POST',
    headers: authHeaders(),
    body: JSON.stringify({
      userName: newPlayerName.value.trim(),
      teamId: selectedTeamId.value || null,
    }),
  })
  newPlayerName.value = ''
  selectedTeamId.value = null
  await fetchGame()
}

async function onAssignTeam(playerId: string, teamId: string | null) {
  const g = await api.assignTeam(playerId, teamId)
  if (g)
    game.value = g
async function addScore() {
  if (!scorePlayerId.value) return
  await fetch(`/api/games/${props.id}/scores`, {
    method: 'POST',
    headers: authHeaders(),
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
  await fetch(`/api/games/${props.id}/start`, { method: 'PUT', headers: authHeaders() })
  await fetchGame()
}

async function onStart() {
  const g = await api.startGame()
  if (g)
    game.value = g
}

/* -- Funktioner för ett aktivt spel -- */
async function onAdvanceRound() {
  const g = await api.advanceRound()
  if (g)
    game.value = g
}

async function onPause() {
  const g = await api.pauseGame()
  if (g)
    game.value = g
}

async function onFinish() {
  const g = await api.finishGame()
  if (g)
    game.value = g
}

async function onAdjustScore(playerId: string, round: number, delta: number) {
  const existing = state.scoreMatrix.value[playerId]?.[round]
async function finishGame() {
  await fetch(`/api/games/${props.id}/finish`, { method: 'PUT', headers: authHeaders() })
  await fetchGame()
}

const rounds = computed(() => {
  if (!game.value) return []
  const maxFromScores = game.value.scores.reduce((m, s) => Math.max(m, s.round ?? 0), 0)
  const maxRound = Math.max(maxFromScores + 1, game.value.maxRounds ?? 1)
  return Array.from({ length: maxRound }, (_, i) => i + 1)
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

function playerTotal(playerId: string): number {
  return game.value?.scores
    .filter(s => s.playerId === playerId)
    .reduce((sum, s) => sum + s.value, 0) ?? 0
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


async function adjustScore(playerId: string, round: number, delta: number) {
  const existing = scoreMatrix.value[playerId]?.[round]
  if (existing) {
    const g = await api.updateScore(playerId, round, existing.value + delta)
    if (g)
      game.value = g
    await fetch(`/api/games/${props.id}/scores`, {
      method: 'PUT',
      headers: authHeaders(),
      body: JSON.stringify({ playerId, round, value: existing.value + delta }),
    })
  } else {
    const teamId = game.value?.players.find(p => p.playerId === playerId)?.teamId ?? null
    const g = await api.addScore(playerId, teamId, round, delta)
    if
    (g) game.value = g
  }
}

/* -- Permissions helper (passad av ScoreMatrix) -- */
function canEditPlayer(playerId: string): boolean {
  if (!game.value)
    return false

  if (hub.claim.value?.playerId === playerId)
    return true

  if (game.value.creatorOnly)
    return state.isCreator.value

  return true
}

/* -- Share link -- */
    await fetch(`/api/games/${props.id}/scores`, {
      method: 'POST',
      headers: authHeaders(),
      body: JSON.stringify({ playerId, teamId, round, value: delta }),
    })
  }
  await fetchGame()
}

const winnerName = computed(() => {
  if (!game.value?.winnerId) return null
  const p = game.value.players.find((pl) => pl.playerId === game.value!.winnerId)
  return p?.playerName ?? 'Okänd'
})

function statusBadge(status: string) {
  return `badge badge-${status.toLowerCase()}`
}

async function claimPlayer(playerId: string | null) {
  showClaimPicker.value = false
  await connectHub(playerId ?? undefined)
}

async function copyShareLink() {
  try {
    await navigator.clipboard.writeText(shareUrl.value)
    alert('Länk till match kopierad!')
  } catch (err) {
    alert('Kunde inte kopiera länken. Här är den: ' + shareUrl.value)
  }
}

async function shareLink() {
  if (navigator.share) {
    await navigator.share({
      title: `Match ${game.value?.name}`,
      text: 'Gå med i spelet!',
      url: state.shareUrl.value
    })
  } else {
    try {
      await navigator.clipboard.writeText(state.shareUrl.value)
      alert('Länk kopierad!')
    } catch {
      alert(state.shareUrl.value)
    }
  }
}

/* -- Claim handlers -- */
async function onClaim(playerId: string | null) {
  await hub.claimPlayer(playerId, refresh)
}

async function onUnclaim() {
  await hub.unclaimPlayer()
}

/* -- Lifecycle -- */
onMounted(async () => {
  api.loading.value = true
  try {
    await refresh()
  } finally {
    api.loading.value = false
  }

  const stored = localStorage.getItem(`claim:${props.id}`)
  if (stored) {
    const parsed = JSON.parse(stored)
    await hub.connect(refresh, parsed.playerId ?? undefined)
  } else {
    hub.showClaimPicker.value = true
    await hub.connect(refresh)
  }
})

onBeforeUnmount(async () => {
  await hub.disconnect()
})
</script>

<template>
  <div class="page">
    <div v-if="isLocked" class="card">
      <h2>{{ lockedGameName || 'Privat match' }}</h2>
      <p>Denna match är lösenordsskyddad. Ange lösenordet för att fortsätta.</p>
      <form @submit.prevent="unlock" class="form-col">
        <input type="password" v-model="lockPassword" placeholder="Lösenord" required />
        <button type="submit" class="btn-primary">Lås upp</button>
      </form>
    </div>

    <template v-else>
    <!-- <router-link to="/" class="back-link">← Tillbaka</router-link> -->

    <div v-if="api.loading.value" class="card">Laddar...</div>
    <PageNotFound v-else-if="!game" />

    <template v-else>
      <!-- Header (alltid synlig) -->
      <GameHeader :game="game" :winner-name="state.winnerName.value"/>

      <!-- Claim picker / info -->
      <ClaimPicker
        :players="game.players"
        :can-switch-claim="state.canSwitchClaim.value"
        :claim="hub.claim.value"
        :show-picker="hub.showClaimPicker.value"
        :connection-id="hub.connection.value?.connectionId ?? null"
        @claim="onClaim"
        @unclaim="onUnclaim"
      />

      <!-- === WAITING | LOBBY === -->
      <GameLobby
        v-if="game.status === 'Waiting'"
        :game="game"
        :is-creator="state.isCreator.value"
        @save="onSaveSettings"
        @add-team="onAddTeam"
        @add-player="onAddPlayer"
        @assign-team="onAssignTeam"
        @start="onStart"
        @share="shareLink"
      />

      <!-- === ACTIVE === -->
      <template v-if="game.status === 'Active'">
        <GameControls
          :game="game"
          :can-edit="state.canEdit.value"
          :is-creator="state.isCreator.value"
          @advance-round="onAdvanceRound"
          @pause="onPause"
          @finish="onFinish"
          @share="shareLink"
        />

        <Scoreboard
          :scoreboard="state.scoreboard.value"
          :has-teams="game.teams.length > 0"
          :starting-score="game.startingScore"
          :winner-id="null"
          :show-crown="true"
        />

        <ScoreMatrix
          :players="game.players"
          :rounds="state.rounds.value"
          :current-round="game.currentRound"
          :score-increment="game.scoreIncrement"
          :starting-score="game.startingScore"
          :readonly="false"
          :score-matrix="state.scoreMatrix.value"
          :can-edit-player="canEditPlayer"
          :get-score-value="state.getScoreValue"
          :player-display-total="state.playerDisplayTotal"
          @adjust="onAdjustScore"
        />
      </template>

      <!-- === FINISHED === -->
      <template v-if="game.status === 'Finished'">
        <Scoreboard
          :scoreboard="state.scoreboard.value"
          :has-teams="game.teams.length > 0"
          :starting-score="game.startingScore"
          :winner-id="game.winnerId"
        >
          <template #title>Slutställning</template>
        </Scoreboard>

        <ScoreMatrix
          :players="game.players"
          :rounds="state.rounds.value"
          :current-round="game.currentRound"
          :score-increment="game.scoreIncrement"
          :starting-score="game.startingScore"
          :readonly="true"
          :score-matrix="state.scoreMatrix.value"
          :can-edit-player="() => false"
          :get-score-value="state.getScoreValue"
          :player-display-total="state.playerDisplayTotal"
        />
      </template>
    </template>
    </template>
  </div>
</template>

