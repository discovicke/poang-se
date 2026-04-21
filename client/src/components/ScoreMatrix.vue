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

    <div class="matrix-scroll-wrapper glass-card">
      <table class="score-matrix">
        <thead>
          <tr>
            <th class="round-col label-sm text-outline">Runda</th>
            <th v-for="p in players" :key="p.playerId" class="player-col label-sm text-primary">
              {{ p.playerName }}
            </th>
          </tr>
        </thead>
        <tbody>
          <tr 
            v-for="r in rounds" 
            :key="r" 
            class="matrix-row"
            :class="{ 'active-round': r === currentRound, 'historical': r < currentRound }"
          >
            <td class="round-label">
              <span class="headline-sm" :class="r === currentRound ? 'text-secondary' : 'text-on-surface-variant'">
                {{ r }}
              </span>
              <span v-if="r === currentRound && !readonly" class="active-indicator"></span>
            </td>
            
            <td v-for="p in players" :key="p.playerId" class="score-cell-td">
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
              <span v-else class="sc-val-readonly font-headline" :class="{ 'empty-cell': getScoreValue(p.playerId, r) === 0 && r > currentRound }">
                {{ getScoreValue(p.playerId, r) }}
              </span>
            </td>
          </tr>
        </tbody>
        <tfoot>
          <tr class="footer-row">
            <td class="round-label label-sm text-outline">
              {{ startingScore ? 'Kvar' : 'Total' }}
            </td>
            <td v-for="p in players" :key="p.playerId" class="total-val headline-sm text-primary">
              {{ playerDisplayTotal(p.playerId) }}
            </td>
          </tr>
        </tfoot>
      </table>
    </div>
  </div>
</template>

<style scoped>
.matrix-container { width: 100%; }
.matrix-scroll-wrapper { overflow-x: auto; padding: 16px; border-radius: var(--radius-2xl); }
.score-matrix { width: 100%; border-collapse: separate; border-spacing: 0 8px; }
.score-matrix th { padding: 12px 16px; text-align: center; }

.round-col { text-align: left !important; min-width: 80px; }
.player-col { min-width: 140px; }

.matrix-row { transition: all 200ms; }
.matrix-row.active-round { background-color: rgba(255, 215, 9, 0.05); }
.matrix-row.historical { opacity: 0.8; }

.score-matrix td { padding: 8px 12px; text-align: center; vertical-align: middle; }

.round-label { text-align: left !important; position: relative; }

.active-indicator {
  position: absolute;
  left: -8px;
  top: 50%;
  transform: translateY(-50%);
  width: 4px;
  height: 24px;
  background-color: var(--color-secondary);
  border-radius: var(--radius-full);
}

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

.footer-row { border-top: 1px solid var(--color-outline-variant); }
.footer-row td { padding-top: 24px; }

/* Utils */
.text-on-surface { color: var(--color-on-surface); }
.text-on-surface-variant { color: var(--color-on-surface-variant); }
.text-primary { color: var(--color-primary); }
.text-secondary { color: var(--color-secondary); }
.text-outline { color: var(--color-outline); }
.mb-lg { margin-bottom: 24px; }
</style>
