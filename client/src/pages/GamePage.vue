<script setup lang="ts">
import {ref, onMounted, onBeforeUnmount, computed} from 'vue'
import type {Game} from '../types/game'
import {useRouter} from 'vue-router'

import {useGameApi} from '../composables/useGameApi'
import {useSignalR} from '../composables/useSignalR'
import {useGameState} from '../composables/useGameState'

import PageNotFound from './PageNotFound.vue'
import ClaimPicker from '../components/ClaimPicker.vue'
import GameLobby from '../components/GameLobby.vue'
import GameControls from '../components/GameControls.vue'
import Scoreboard from '../components/Scoreboard.vue'
import ScoreMatrix from '../components/ScoreMatrix.vue'
import ScoreCharts from '../components/ScoreCharts.vue'

const props = defineProps<{ id: string }>()
const router = useRouter()

/* -- Composables -- */
const api = useGameApi(props.id)
const hub = useSignalR(props.id)

const game = ref<Game | null>(null)
const state = useGameState(game, props.id)

const isLastRound = computed(() => {
  if (!game.value) return false
  if (game.value.gameMode === 'BestOf' || game.value.gameMode === 'FirstTo') return false
  if (game.value.maxRounds === null) return false
  return game.value.currentRound >= game.value.maxRounds
})

/* -- Refresh helper -- */
async function refresh() {
  const g = await api.fetchGame()
  if (g) game.value = g
}

/* -- Lobby Actions -- */
async function onSaveSettings(settings: Record<string, unknown>) {
  const secret = localStorage.getItem(`creator:${props.id}`)
  if (!secret) return
  const g = await api.saveSettings(secret, settings as any)
  if (g) game.value = g
}

async function onAddTeam(name: string) {
  const g = await api.addTeam(name)
  if (g) game.value = g
}

async function onRemoveTeam(teamId: string) {
  const g = await api.removeTeam(teamId)
  if (g) game.value = g
}

async function onAddPlayer(name: string, teamId: string | null) {
  const g = await api.addPlayer(name, teamId)
  if (g) game.value = g
}

async function onAssignTeam(playerId: string, teamId: string | null) {
  const g = await api.assignTeam(playerId, teamId)
  if (g) game.value = g
}

async function onRemovePlayer(playerId: string) {
  const g = await api.removePlayer(playerId)
  if (g) game.value = g
}

async function onRenamePlayer(playerId: string, name: string) {
  const g = await api.renamePlayer(playerId, name)
  if (g) game.value = g
}

async function onRenameTeam(teamId: string, name: string) {
  const g = await api.renameTeam(teamId, name)
  if (g) game.value = g
}

/* -- Game Flow Actions -- */
async function onStart() {
  const g = await api.startGame()
  if (g) game.value = g
}

async function onResume() {
  const g = await api.resumeGame()
  if (g) game.value = g
}

async function onAdvanceRound() {
  const g = await api.advanceRound()
  if (g) game.value = g
}

async function onPause() {
  const g = await api.pauseGame()
  if (g) game.value = g
}

async function onFinish() {
  const g = await api.finishGame()
  if (g) game.value = g
}

async function onReset() {
  const g = await api.resetGame()
  if (g) game.value = g
}

async function onRematch() {
  const result = await api.rematch()
  if (!result) return
  localStorage.setItem(`creator:${result.id}`, result.creatorSecret)
  await router.push(`/games/${result.id}`)
}

/* -- Score Actions -- */
async function onAdjustScore(playerId: string, round: number, delta: number) {
  const existing = state.scoreMatrix.value[playerId]?.[round]
  if (existing) {
    const g = await api.updateScore(playerId, round, existing.value + delta)
    if (g) game.value = g
  } else {
    const teamId = game.value?.players.find(p => p.playerId === playerId)?.teamId ?? null
    const g = await api.addScore(playerId, teamId, round, delta)
    if (g) game.value = g
  }
}

async function onUpdateScore(playerId: string, round: number, newValue: number) {
  const existing = state.scoreMatrix.value[playerId]?.[round]
  if (existing) {
    const g = await api.updateScore(playerId, round, newValue)
    if (g) game.value = g
  } else {
    const teamId = game.value?.players.find(p => p.playerId === playerId)?.teamId ?? null
    const g = await api.addScore(playerId, teamId, round, newValue)
    if (g) game.value = g
  }
}

function canEditPlayer(playerId: string): boolean {
  if (!game.value) return false

  // Spelskaparen kan redigera alla
  if (state.isCreator.value) return true

  // Om spelet inte är låst till creatorOnly, kan jag redigera MIN spelare
  if (!game.value.creatorOnly && hub.claim.value?.playerId === playerId) return true

  return false
}

const canSwitchClaim = computed(() => game.value?.status === 'Waiting')

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
    } catch {
      alert(state.shareUrl.value)
    }
  }
}

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

async function connectHub() {
  const stored = localStorage.getItem(`claim:${props.id}`)
  if (stored) {
    const parsed = JSON.parse(stored)
    await hub.connect(refresh, parsed.playerId ?? undefined)
  } else {
    await hub.connect(refresh)
  }
}

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
  <div class="game-page">
    <div v-if="api.loading.value" class="loading-state">
      <div class="loader"></div>
      <p class="label-sm">Laddar galleriet...</p>
    </div>

    <!-- Locked State -->
    <div v-else-if="api.isLocked.value" class="locked-container">
      <div class="glass-card locked-card">
        <span class="material-symbols-outlined locked-icon">lock</span>
        <h2 class="headline-md">{{ api.lockedGameName.value || 'Privat match' }}</h2>
        <p class="body-md text-on-surface-variant">Den här matchen är lösenordsskyddad.</p>

        <form @submit.prevent="onUnlock" class="unlock-form">
          <input
            v-model="unlockPassword"
            type="password"
            placeholder="Lösenord"
            class="primary-input"
            required
            autofocus
          />
          <p v-if="unlockError" class="error-text">Fel lösenord, försök igen.</p>
          <button type="submit" class="submit-btn">Lås upp</button>
        </form>
      </div>
    </div>

    <PageNotFound v-else-if="!game"/>

    <template v-else>
      <main class="game-container">
        <!-- Main Content (Scoreboard & Matrix) -->
        <div class="main-content">
          <div class="content-header">
            <div>
              <span class="label-sm text-on-surface-variant">Pågående Match</span>
              <h2 class="headline-md text-primary">{{ game.name }}</h2>
            </div>
            <div class="header-actions">
              <button class="icon-btn" @click="shareLink" title="Dela"><span
                class="material-symbols-outlined">share</span> Dela spel
              </button>
            </div>
          </div>

          <div class="scoreboard-grid">
            <!-- Left Column: Core Gameplay -->
            <div class="score-column">
              <!-- Mobile ClaimPicker (hidden on desktop where sidebar handles it) -->
              <div class="mobile-claim mobile-only">
                <ClaimPicker
                  :players="game.players"
                  :can-switch-claim="canSwitchClaim"
                  :claim="hub.claim.value"
                  :show-picker="hub.showClaimPicker.value"
                  :connection-id="hub.connection.value?.connectionId ?? null"
                  @claim="onClaim"
                  @unclaim="onUnclaim"
                />
              </div>
              <!-- Show Lobby if Waiting -->
              <div v-if="game.status === 'Waiting'" class="mt-lg">
                <GameLobby
                  :game="game"
                  :is-creator="state.isCreator.value"
                  @save="onSaveSettings"
                  @add-team="onAddTeam"
                  @remove-team="onRemoveTeam"
                  @add-player="onAddPlayer"
                  @assign-team="onAssignTeam"
                  @remove-player="onRemovePlayer"
                  @rename-player="onRenamePlayer"
                  @rename-team="onRenameTeam"
                  @start="onStart"
                  @resume="onResume"
                  @share="shareLink"
                />
              </div>

              <!-- Show Gameplay if Active or Finished -->
              <template v-else>
                <Scoreboard
                  :scoreboard="state.scoreboard.value"
                  :has-teams="game.teams.length > 0"
                  :starting-score="game.startingScore"
                  :winner-id="game.status === 'Finished' ? game.winnerId : null"
                  :show-crown="true"
                />

                <div class="matrix-section mt-xl">
                  <ScoreMatrix
                    :players="game.players"
                    :rounds="state.rounds.value"
                    :current-round="game.currentRound"
                    :score-increment="game.scoreIncrement"
                    :starting-score="game.startingScore"
                    :readonly="game.status === 'Finished'"
                    :score-matrix="state.scoreMatrix.value"
                    :creator-only="game.creatorOnly"
                    :is-creator="state.isCreator.value"
                    :can-edit-player="canEditPlayer"
                    :get-score-value="state.getScoreValue"
                    :player-display-total="state.playerDisplayTotal"
                    @adjust="onAdjustScore"
                    @update="onUpdateScore"
                  />
                </div>

                <div v-if="game.status === 'Finished'" class="mt-xl">
                  <ScoreCharts :gameId="game.id" :starting-score="game.startingScore" />
                </div>
              </template>
            </div>

            <!-- Right Column: Context & Controls (Desktop only) -->
            <aside class="management-column md-only">
              <!-- Active Controls -->
              <div v-if="game.status === 'Active'" class="glass-card status-card">
                <GameControls
                  :game="game"
                  :can-edit="state.canEdit.value"
                  :is-creator="state.isCreator.value"
                  @advance-round="onAdvanceRound"
                  @pause="onPause"
                  @finish="onFinish"
                  @share="shareLink"
                />
              </div>

              <!-- Finished Controls -->
              <div v-if="game.status === 'Finished'" class="glass-card status-card">
                <div class="finished-controls">
                  <h3 class="headline-sm text-secondary mb-md">Matchen är slut!</h3>
                  <div class="flex-col gap-sm">
                    <button class="secondary-btn w-full" @click="onReset">
                      <span class="material-symbols-outlined">refresh</span>
                      STARTA OM MATCHEN
                    </button>
                    <button class="primary-btn w-full mt-sm" @click="onRematch">
                      <span class="material-symbols-outlined">content_copy</span>
                      NY MATCH (SAMMA INST.)
                    </button>
                  </div>
                </div>
              </div>

              <!-- Claim Info -->
              <div class="claim-card mt-lg">
                <ClaimPicker
                  :players="game.players"
                  :can-switch-claim="canSwitchClaim"
                  :claim="hub.claim.value"
                  :show-picker="hub.showClaimPicker.value"
                  :connection-id="hub.connection.value?.connectionId ?? null"
                  @claim="onClaim"
                  @unclaim="onUnclaim"
                />
              </div>
            </aside>
          </div>
        </div>
      </main>

      <!-- Mobile Controls Overlay (FAB style) -->
      <div v-if="game.status === 'Active'" class="mobile-controls mobile-only">
        <button v-if="state.isCreator.value && !isLastRound" @click="onPause" class="fab-secondary" title="Pausa">
          <span class="material-symbols-outlined">pause</span>
        </button>
        <button
          v-if="state.canEdit.value"
          @click="isLastRound ? onFinish() : onAdvanceRound()"
          class="fab-main"
          :class="{ 'fab-main--finish': isLastRound }"
          :title="isLastRound ? 'Avsluta match' : 'Nästa runda'"
        >
          <span class="material-symbols-outlined">{{ isLastRound ? 'stop' : 'fast_forward' }}</span>
        </button>
      </div>
    </template>
  </div>
</template>

<style scoped>
.game-page {
  min-height: calc(100vh - 72px);
  background-color: var(--color-surface);
}

.loading-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  height: 60vh;
  gap: 16px;
}

.loader {
  width: 48px;
  height: 48px;
  border: 4px solid var(--color-surface-container-high);
  border-top-color: var(--color-primary);
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

/* Locked State */
.locked-container {
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 48px 24px;
}

.locked-card {
  max-width: 480px;
  width: 100%;
  padding: 48px;
  text-align: center;
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.locked-icon {
  font-size: 64px;
  color: var(--color-primary);
  margin-bottom: 8px;
}

.unlock-form {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.primary-input {
  background-color: var(--color-surface-container-high);
  border: none;
  border-radius: var(--radius-xl);
  padding: 16px 24px;
  color: var(--color-on-surface);
  font-size: 18px;
  text-align: center;
}

.submit-btn {
  background-color: var(--color-primary);
  color: var(--color-on-primary-fixed);
  border: none;
  border-radius: var(--radius-xl);
  padding: 16px;
  font-weight: 700;
  cursor: pointer;
  transition: all 200ms;
}

.submit-btn:hover {
  box-shadow: 0 0 20px rgba(132, 173, 255, 0.3);
}

/* Main Layout */
.game-container {
  padding: 24px;
}

@media (min-width: 768px) {
  .game-container {
    padding: 48px;
  }
}

.main-content {
  max-width: 1400px;
  margin: 0 auto;
}

.content-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-end;
  margin-bottom: 32px;
}

.header-actions {
  display: flex;
  gap: 8px;
}

.icon-btn {
  background: transparent;
  border: none;
  color: var(--color-on-surface-variant);
  padding: 8px;
  border-radius: var(--radius-full);
  cursor: pointer;
  transition: all 200ms;
}

.icon-btn:hover {
  background-color: var(--color-surface-container-high);
  color: var(--color-primary);
}

.icon-btn:active {
  transform: scale(0.9);
  opacity: 0.7;
}

.scoreboard-grid {
  display: grid;
  grid-template-columns: 1fr;
  gap: 32px;
}

@media (min-width: 1024px) {
  .scoreboard-grid {
    grid-template-columns: 8fr 4fr;
  }
}

/* Management Column */
.status-card {
  padding: 24px;
}

.atmospheric-image {
  position: relative;
  height: 200px;
  border-radius: var(--radius-xl);
  overflow: hidden;
}

.atmospheric-image img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  filter: grayscale(1);
  transition: filter 500ms;
}

.atmospheric-image:hover img {
  filter: grayscale(0);
}

.img-overlay {
  position: absolute;
  inset: 0;
  background: linear-gradient(to top, var(--color-background), transparent);
  padding: 24px;
  display: flex;
  flex-direction: column;
  justify-content: flex-end;
}

/* Buttons */
.primary-btn {
  background-color: var(--color-primary);
  color: var(--color-on-primary-fixed);
  border: none;
  padding: 16px;
  border-radius: var(--radius-xl);
  font-weight: 700;
  cursor: pointer;
}

.secondary-btn {
  background: transparent;
  border: 1px solid var(--color-outline-variant);
  color: var(--color-on-surface);
  padding: 16px;
  border-radius: var(--radius-xl);
  font-weight: 700;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
}

.secondary-btn:hover {
  background-color: var(--color-surface-container-high);
}

/* Extra scroll room so last round card clears the mobile FABs */
.matrix-section {
  padding-bottom: 180px;
}

@media (min-width: 768px) {
  .matrix-section {
    padding-bottom: 0;
  }
}

/* Mobile FAB */
.mobile-controls {
  position: fixed;
  bottom: 88px;
  right: 24px;
  z-index: 100;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 12px;
}

.fab-main {
  width: 64px;
  height: 64px;
  border-radius: 50%;
  background-color: var(--color-primary);
  color: var(--color-on-primary-fixed);
  border: none;
  box-shadow: 0 0 40px rgba(132, 173, 255, 0.4);
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
}

.fab-main span {
  font-size: 32px;
}

.fab-main--finish {
  background-color: var(--color-error);
  box-shadow: 0 0 40px rgba(255, 80, 80, 0.4);
}

.fab-secondary {
  width: 48px;
  height: 48px;
  border-radius: 50%;
  background-color: var(--color-surface-container-high);
  color: var(--color-on-surface-variant);
  border: 1px solid var(--color-outline-variant);
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: all 200ms;
}

.fab-secondary:hover {
  color: var(--color-secondary);
  border-color: var(--color-secondary);
}

/* Utils */
.text-primary {
  color: var(--color-primary);
}

.text-secondary {
  color: var(--color-secondary);
}

.mb-md {
  margin-bottom: 16px;
}

.mt-lg {
  margin-top: 24px;
}

.mt-xl {
  margin-top: 32px;
}

.w-full {
  width: 100%;
}

.flex-col {
  display: flex;
  flex-direction: column;
}

.gap-sm {
  gap: 8px;
}

/* Visibility Utilities */
.mobile-only {
  display: block;
}

.md-only {
  display: none;
}

@media (min-width: 768px) {
  .mobile-only {
    display: none;
  }

  .md-only {
    display: block;
  }
}

.mobile-claim {
  margin-bottom: 24px;
}
</style>
