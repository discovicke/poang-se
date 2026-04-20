<script setup lang="ts">
import type {Game} from '../types/game'
import {useRouter} from 'vue-router'

defineProps<{
  game: Game
  winnerName: string | null
}>()

const router = useRouter()

function statusBadge(status: string) {
  return `badge badge-${status.toLowerCase()}`
}
</script>

<template>
  <div>
    <div class="header-top">
      <button class="btn-sm btn-secondary back-btn" @click="router.push('/')"><- Startsida</button>
      <h1>{{ game.name }}</h1>
    </div>
    <div class="game-meta">
      <span>Status: <span :class="statusBadge(game.status)">{{ game.status }}</span></span>
      <span v-if="game.startingScore">Start: <strong>{{ game.startingScore }}</strong></span>
      <span>Lägre = bättre:
        <strong>
          {{
            game.lowerIsBetter
              ? 'Ja'
              : 'Nej'
          }}
        </strong>
      </span>
      <span>Rundor: <strong>{{ game.maxRounds ?? '∞' }}</strong></span>
      <span>Inkrement: <strong>{{ game.scoreIncrement }}</strong></span>
      <span v-if="game.gameMode">
        Läge:
        <strong>
          {{
            game.gameMode === 'BestOf'
              ? `Bäst av ${game.gameModeValue}`
              : `Först till ${game.gameModeValue}`
          }}
        </strong>
      </span>

      <span v-if="game.isTemporary && game.expiresAt">
         <br/><strong>⚠️ Tillfälligt spel!</strong>
         <span><strong> Avslutas {{ new Date(game.expiresAt).toLocaleString('sv-SE') }}</strong></span>
      </span>
    </div>

    <!-- Winner banner -->
    <div v-if="game.status === 'Finished'" class="winner-banner">
      <strong>🏆 Spelet avslutat!</strong>
      <span v-if="winnerName"> Vinnare: <strong>{{ winnerName }}</strong></span>
      <span v-if="game.teamBasedWinner"> (Lagvinst)</span>
      <br/><small class="text-muted">{{ new Date(game.finishedAt!).toLocaleString('sv-SE') }}</small>
    </div>
  </div>
</template>

