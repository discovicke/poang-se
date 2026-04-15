<script setup lang="ts">
import type {GamePlayer, ScoreEntry} from '../types/game'

const props = defineProps<{
  players: GamePlayer[]
  rounds: number[]
  currentRound: number
  scoreIncrement: number
  startingScore: number
  readonly: boolean
  scoreMatrix: Record<string, Record<number, ScoreEntry>>

  /** Funktion för att kontrollera om en specifik spelares poäng kan redigeras */
  canEditPlayer: (playerId: string) => boolean
  /** Hämtar poängvärdet för spelare/runda */
  getScoreValue: (playerId: string, round: number) => number
  /** Hämtar visningstotal för spelare */
  playerDisplayTotal: (playerId: string) => number
}>()

const emit = defineEmits<{
  (e: 'adjust', playerId: string, round: number, delta: number): void
}>()
</script>

<template>
  <div v-if="players.length" class="card">
    <h2>{{
        readonly
          ? 'Poänghistorik'
          : 'Poängmatris'
      }}</h2>
    <div class="score-matrix-wrapper">
      <table class="score-matrix">
        <thead>
        <tr>
          <th class="round-col">Runda</th>
          <th v-for="p in players" :key="p.playerId" class="player-col">{{ p.playerName }}</th>
        </tr>
        </thead>
        <tbody>
        <tr v-for="r in rounds" :key="r" :class="{ 'active-round': !readonly && r === currentRound }">
          <td class="round-label">
            Runda {{ r }}
            <span v-if="!readonly && r === currentRound" class="round-badge">←</span>
          </td>
          <td v-for="p in players" :key="p.playerId" class="score-cell-td">
            <div v-if="!readonly && canEditPlayer(p.playerId)" class="score-cell">
              <button class="sc-btn sc-minus" @click="emit('adjust', p.playerId, r, -scoreIncrement)">−</button>
              <span class="sc-val">{{ getScoreValue(p.playerId, r) }}</span>
              <button class="sc-btn sc-plus" @click="emit('adjust', p.playerId, r, scoreIncrement)">+</button>
            </div>
            <span v-else class="sc-val-readonly">{{ getScoreValue(p.playerId, r) }}</span>
          </td>
        </tr>
        </tbody>
        <tfoot>
        <tr>
          <td class="round-label">{{
              startingScore
                ? 'Kvar'
                : 'Total'
            }}
          </td>
          <td v-for="p in players" :key="p.playerId" class="total-val">
            {{ playerDisplayTotal(p.playerId) }}
          </td>
        </tr>
        </tfoot>
      </table>
    </div>
  </div>
</template>

