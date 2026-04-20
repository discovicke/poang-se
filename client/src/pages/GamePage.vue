<script setup lang="ts">
import {ref, onMounted, onBeforeUnmount, computed} from 'vue'
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
}

async function onAddPlayer(name: string, teamId: string | null) {
  const g = await api.addPlayer(name, teamId)
  if (g)
    game.value = g
}

async function onAssignTeam(playerId: string, teamId: string | null) {
  const g = await api.assignTeam(playerId, teamId)
  if (g)
    game.value = g
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
  if (existing) {
    const g = await api.updateScore(playerId, round, existing.value + delta)
    if (g)
      game.value = g
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

/* -- Claim switch permission: endast möjligt under Waiting-status -- */
const canSwitchClaim = computed(() => game.value?.status === 'Waiting')

/* -- Share link -- */
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

/* -- Unlock -- */
const unlockPassword = ref('')
const unlockError = ref(false)

async function onUnlock() {
  unlockError.value = false
  const ok = await api.unlockGame(unlockPassword.value)
  if (ok) {
    unlockPassword.value = ''
    await refresh()
    connectHub()
  } else {
    unlockError.value = true
  }
}

/* -- Hub connect helper (reused after unlock) -- */
async function connectHub() {
  const stored = localStorage.getItem(`claim:${props.id}`)
  if (stored) {
    const parsed = JSON.parse(stored)
    await hub.connect(refresh, parsed.playerId ?? undefined)
  } else {
    // Ingen sparad identitet: anslut utan roll. Servern skickar ClaimPending
    // och useSignalR visar picker när det händer.
    await hub.connect(refresh)
  }
}

/* -- Lifecycle -- */
onMounted(async () => {
  api.loading.value = true
  try {
    await refresh()
  } finally {
    api.loading.value = false
  }

  if (!api.isLocked.value) {
    await connectHub()
  }
})

onBeforeUnmount(async () => {
  await hub.disconnect()
})
</script>

<template>
  <div class="page">
    <!-- <router-link to="/" class="back-link">← Tillbaka</router-link> -->

    <div v-if="api.loading.value" class="card">Laddar...</div>

    <!-- Lösenordsskyddad match -->
    <div v-else-if="api.isLocked.value" class="card">
      <h2>{{ api.lockedGameName.value ?? 'Privat match' }}</h2>
      <p>Den här matchen är lösenordsskyddad.</p>
      <form @submit.prevent="onUnlock" class="form-col">
        <input
          v-model="unlockPassword"
          type="password"
          placeholder="Lösenord"
          required
          autofocus
        />
        <p v-if="unlockError" class="error">Fel lösenord, försök igen.</p>
        <button type="submit" class="btn-primary">Lås upp</button>
      </form>
    </div>

    <PageNotFound v-else-if="!game" />

    <template v-else>
      <!-- Header (alltid synlig) -->
      <GameHeader :game="game" :winner-name="state.winnerName.value"/>

      <!-- Claim picker / info -->
      <ClaimPicker
        :players="game.players"
        :can-switch-claim="canSwitchClaim"
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
  </div>
</template>

