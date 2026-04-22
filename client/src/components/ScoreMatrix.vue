<script setup lang="ts">
import type { GamePlayer, ScoreEntry } from '../types/game'

const props = defineProps<{
  players: GamePlayer[]
  rounds: number[]
  currentRound: number
  scoreIncrement: number
  startingScore: number
  readonly: boolean
  scoreMatrix: Record<string, Record<number, ScoreEntry>>
  creatorOnly: boolean
  isCreator: boolean

  /** Funktion för att kontrollera om en specifik spelares poäng kan redigeras */
  canEditPlayer: (playerId: string) => boolean
  /** Hämtar poängvärdet för spelare/runda */
  getScoreValue: (playerId: string, round: number) => number
  /** Hämtar visningstotal för spelare */
  playerDisplayTotal: (playerId: string) => number
}>()

const emit = defineEmits<{
  (e: 'adjust', playerId: string, round: number, delta: number): void
  (e: 'update', playerId: string, round: number, newValue: number): void
}>()

function canEditCell(playerId: string, round: number): boolean {
  if (props.readonly) return false

  // Admin kan alltid redigera allt (historik också)
  if (props.isCreator) return true

  // Vanliga användare kan bara redigera aktuell runda...
  if (round !== props.currentRound) return false

  // ...om inte "Bara admin" är påslaget
  return props.canEditPlayer(playerId)
}

function onManualInput(playerId: string, round: number, event: Event) {
  const input = event.target as HTMLInputElement
  const val = parseInt(input.value)
  if (!isNaN(val)) {
    emit('update', playerId, round, val)
  }
}
</script>

<template>
  <div v-if="players.length" class="matrix-container">
    <div class="header-row mb-lg">
      <h2 class="headline-md text-on-surface">
        {{ readonly ? 'Matchhistorik' : 'Poängmatris' }}
      </h2>
      <div v-if="!readonly" class="label-xs text-on-surface-variant italic">
        {{ isCreator ? 'Admin-läge: Du kan redigera all historik' : 'Klicka på runda ' + currentRound + ' för att ändra dina poäng' }}
      </div>
    </div>

    <!-- Round cards (all screen sizes) -->
    <div class="round-cards">
      <div
        v-for="r in rounds"
        :key="r"
        class="round-card glass-card"
        :class="{ 'round-card--active': r === currentRound, 'round-card--historical': r < currentRound }"
      >
        <div class="round-card-header">
          <div class="round-card-title">
            <span class="active-indicator-pill" v-if="r === currentRound && !readonly"></span>
            <span class="headline-sm" :class="r === currentRound ? 'text-secondary' : 'text-on-surface-variant'">Runda {{ r }}</span>
            <span v-if="r === currentRound && !readonly" class="round-badge">Pågående</span>
          </div>
        </div>

        <div class="round-player-list">
          <div v-for="p in players" :key="p.playerId" class="round-player-row">
            <span class="round-player-name">{{ p.playerName }}</span>

            <div v-if="canEditCell(p.playerId, r)" class="score-input-group">
              <button class="sc-btn sc-minus" @click="emit('adjust', p.playerId, r, -scoreIncrement)">−</button>
              <input
                type="number"
                :value="getScoreValue(p.playerId, r)"
                class="manual-input font-headline"
                @change="onManualInput(p.playerId, r, $event)"
              />
              <button class="sc-btn sc-plus" @click="emit('adjust', p.playerId, r, scoreIncrement)">+</button>
            </div>
            <span
              v-else
              class="sc-val-readonly font-headline"
              :class="{ 'empty-cell': getScoreValue(p.playerId, r) === 0 && r > currentRound }"
            >{{ getScoreValue(p.playerId, r) }}</span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.matrix-container { width: 100%; }

.score-input-group {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  background-color: var(--color-surface-container-high);
  padding: 4px;
  border-radius: var(--radius-lg);
  border: 1px solid rgba(132, 173, 255, 0.1);
}

.manual-input {
  width: 50px;
  background: transparent;
  border: none;
  color: var(--color-on-surface);
  text-align: center;
  font-size: 18px;
  font-weight: 700;
  -moz-appearance: textfield;
}

.manual-input::-webkit-outer-spin-button,
.manual-input::-webkit-inner-spin-button { -webkit-appearance: none; margin: 0; }
.manual-input:focus { outline: none; color: var(--color-primary); }

.sc-btn {
  width: 28px;
  height: 28px;
  border-radius: var(--radius-md);
  border: none;
  font-weight: 900;
  cursor: pointer;
  transition: all 150ms;
  display: flex;
  align-items: center;
  justify-content: center;
  background-color: rgba(255, 255, 255, 0.05);
  color: var(--color-on-surface-variant);
}
.sc-btn:hover { background-color: var(--color-primary); color: var(--color-on-primary-fixed); }

.sc-val-readonly { font-size: 18px; font-weight: 700; color: var(--color-on-surface); }
.empty-cell { opacity: 0.15; }

/* Round cards */
.round-cards {
  display: flex;
  flex-direction: column;
  gap: 12px;
  padding-bottom: 60px;
}

.round-card {
  padding: 20px;
  border-radius: var(--radius-2xl);
  transition: all 200ms;
}

.round-card--active {
  border: 1px solid rgba(255, 215, 9, 0.25);
  background-color: rgba(255, 215, 9, 0.03);
}

.round-card--historical { opacity: 0.75; }

.round-card--totals {
  border: 1px solid rgba(132, 173, 255, 0.2);
}

.round-card-header { margin-bottom: 14px; }

.round-card-title {
  display: flex;
  align-items: center;
  gap: 10px;
}

.active-indicator-pill {
  width: 4px;
  height: 18px;
  background-color: var(--color-secondary);
  border-radius: var(--radius-full);
  flex-shrink: 0;
}

.round-badge {
  font-size: 10px;
  font-weight: 900;
  text-transform: uppercase;
  letter-spacing: 0.12em;
  background-color: rgba(255, 215, 9, 0.15);
  color: var(--color-secondary);
  padding: 2px 8px;
  border-radius: var(--radius-full);
}

.round-player-list {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.round-player-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 12px;
}

.round-player-name {
  font-size: 14px;
  font-weight: 600;
  color: var(--color-on-surface);
  flex: 1;
  min-width: 0;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

/* Utils */
.text-on-surface { color: var(--color-on-surface); }
.text-on-surface-variant { color: var(--color-on-surface-variant); }
.text-primary { color: var(--color-primary); }
.text-secondary { color: var(--color-secondary); }
.text-outline { color: var(--color-outline); }
.mb-lg { margin-bottom: 24px; }
</style>
