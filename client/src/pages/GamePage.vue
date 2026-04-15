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
  </div>
</template>

